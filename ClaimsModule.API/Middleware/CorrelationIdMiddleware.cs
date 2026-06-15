namespace ClaimsModule.API.Middleware;

public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers["X-Correlation-ID"]
            .FirstOrDefault();

        if (string.IsNullOrWhiteSpace(correlationId))
            correlationId = Guid.NewGuid().ToString();

        // Store in context items
        context.Items["CorrelationId"] = Guid.Parse(correlationId);

        // Echo back in response header
        context.Response.Headers["X-Correlation-ID"] = correlationId;

        await _next(context);
    }
}