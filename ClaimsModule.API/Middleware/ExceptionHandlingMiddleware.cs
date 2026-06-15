using ClaimsModule.Domain.Exceptions;
using FluentValidation;
using System.Net;
using System.Text.Json;

namespace ClaimsModule.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        object response;

        switch (exception)
        {
            case ValidationException ve:
                context.Response.StatusCode = (int)HttpStatusCode.UnprocessableEntity;

                // Group errors by field name as spec requires
                var errors = ve.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => string.IsNullOrEmpty(g.Key) ? "General" : g.Key,
                        g => g.Select(e => e.ErrorMessage).ToArray());

                response = new
                {
                    type = "ValidationError",
                    title = "One or more validation errors occurred.",
                    status = (int)HttpStatusCode.UnprocessableEntity,
                    errors
                };
                break;

            case ClaimNotFoundException or ReserveNotFoundException:
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                response = new
                {
                    type = "NotFound",
                    title = exception.Message,
                    status = (int)HttpStatusCode.NotFound,
                    errors = new Dictionary<string, string[]>()
                };
                break;

            case InvalidStatusTransitionException
                or SelfApprovalException
                or InsufficientAuthorityException
                or DomainException:
                context.Response.StatusCode = (int)HttpStatusCode.UnprocessableEntity;
                response = new
                {
                    type = "BusinessRuleViolation",
                    title = exception.Message,
                    status = (int)HttpStatusCode.UnprocessableEntity,
                    errors = new Dictionary<string, string[]>()
                };
                break;

            default:
                _logger.LogError(exception, "Unhandled exception");
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                response = new
                {
                    type = "ServerError",
                    title = "An unexpected error occurred.",
                    status = (int)HttpStatusCode.InternalServerError,
                    errors = new Dictionary<string, string[]>()
                };
                break;
        }

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(response,
                new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                }));
    }
}