namespace ClaimsModule.Application.Common.Interfaces;

public interface IAuditLogService
{
    Task LogAsync(
        Guid claimId,
        Guid organisationId,
        string eventType,
        string description,
        Guid? createdByUserId = null,
        string? oldValue = null,
        string? newValue = null,
        Guid? relatedEntityId = null,
        string? relatedEntityType = null,
        Guid? correlationId = null,
        CancellationToken cancellationToken = default);
}