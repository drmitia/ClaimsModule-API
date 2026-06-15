using ClaimsModule.Application.Common.Interfaces;

namespace ClaimsModule.API.Services;

public class CorrelationIdService : ICorrelationIdService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CorrelationIdService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid CorrelationId
    {
        get
        {
            var context = _httpContextAccessor.HttpContext;

            if (context is null)
                return Guid.NewGuid();

            // Check if already set for this request
            if (context.Items.TryGetValue("CorrelationId", out var existing)
                && existing is Guid existingGuid)
                return existingGuid;

            // Try to read from request header
            var headerValue = context.Request.Headers["X-Correlation-ID"]
                .FirstOrDefault();

            var correlationId = Guid.TryParse(headerValue, out var parsed)
                ? parsed
                : Guid.NewGuid();

            // Store for reuse within this request
            context.Items["CorrelationId"] = correlationId;

            return correlationId;
        }
    }
}