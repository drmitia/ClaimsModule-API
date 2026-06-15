using ClaimsModule.Domain.Enumerations;

namespace ClaimsModule.Application.Common.Interfaces;

public interface ICurrentUserService
{
    Guid UserId { get; }
    string UserName { get; }
    UserRole Role { get; }
    Guid OrganisationId { get; }
}