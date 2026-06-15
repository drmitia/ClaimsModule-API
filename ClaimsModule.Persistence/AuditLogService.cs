using ClaimsModule.Application.Common.Interfaces;
using ClaimsModule.Domain.Entities;

namespace ClaimsModule.Persistence;

public class AuditLogService : IAuditLogService
{
    private readonly ApplicationDbContext _context;

    public AuditLogService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task LogAsync(
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
        CancellationToken cancellationToken = default)
    {
        var entry = new ClaimAuditLog
        {
            AuditLogId = Guid.NewGuid(),
            ClaimId = claimId,
            OrganisationId = organisationId,
            EventType = eventType,
            Description = description,
            CreatedByUserId = createdByUserId,
            OldValue = oldValue,
            NewValue = newValue,
            RelatedEntityId = relatedEntityId,
            RelatedEntityType = relatedEntityType,
            CorrelationId = correlationId,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _context.ClaimAuditLogs.Add(entry);
        await _context.SaveChangesAsync(cancellationToken);
    }
}