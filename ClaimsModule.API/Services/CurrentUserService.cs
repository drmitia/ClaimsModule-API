using ClaimsModule.Application.Common.Interfaces;
using ClaimsModule.Domain.Enumerations;

namespace ClaimsModule.API.Services;

public class CurrentUserService : ICurrentUserService
{
    // Hardcoded mock users as per spec
    // In production this would read from JWT token claims
    private static readonly Dictionary<string, (Guid Id, string Name, UserRole Role)> _mockUsers = new()
    {
        ["handler"] = (Guid.Parse("A0000000-0000-0000-0000-000000000001"), "John Handler", UserRole.Handler),
        ["supervisor"] = (Guid.Parse("A0000000-0000-0000-0000-000000000002"), "Jane Supervisor", UserRole.Supervisor),
        ["manager"] = (Guid.Parse("A0000000-0000-0000-0000-000000000003"), "Bob Manager", UserRole.Manager),
    };

    private static readonly Guid _mockOrganisationId =
        Guid.Parse("B0000000-0000-0000-0000-000000000001");

    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid UserId => GetCurrentUser().Id;
    public string UserName => GetCurrentUser().Name;
    public UserRole Role => GetCurrentUser().Role;
    public Guid OrganisationId => _mockOrganisationId;

    private (Guid Id, string Name, UserRole Role) GetCurrentUser()
    {
        // Read X-User header for mock auth (e.g. "handler", "supervisor", "manager")
        var userKey = _httpContextAccessor.HttpContext?
            .Request.Headers["X-User"].ToString().ToLower() ?? "handler";

        return _mockUsers.TryGetValue(userKey, out var user)
            ? user
            : _mockUsers["handler"];
    }
}