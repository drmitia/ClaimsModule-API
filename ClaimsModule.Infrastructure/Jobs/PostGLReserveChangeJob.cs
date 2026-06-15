using ClaimsModule.Application.Common.Interfaces;
using ClaimsModule.Application.Common;
using Microsoft.Extensions.Logging;

namespace ClaimsModule.Infrastructure.Jobs;

public class PostGLReserveChangeJob
{
    private readonly IApplicationDbContext _context;
    private readonly IAuditLogService _auditLog;
    private readonly ILogger<PostGLReserveChangeJob> _logger;

    public PostGLReserveChangeJob(
        IApplicationDbContext context,
        IAuditLogService auditLog,
        ILogger<PostGLReserveChangeJob> logger)
    {
        _context = context;
        _auditLog = auditLog;
        _logger = logger;
    }

    public async Task ExecuteAsync(
        Guid reserveHistoryId,
        string idempotencyKey,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "GL posting job started for reserve {ReserveHistoryId} with key {IdempotencyKey}",
            reserveHistoryId, idempotencyKey);

        var history = await _context.ReserveHistories
            .FindAsync([reserveHistoryId], cancellationToken);

        if (history is null)
        {
            _logger.LogWarning("Reserve history {Id} not found", reserveHistoryId);
            return;
        }

        // Idempotency check — if already posted, skip
        if (history.PostingStatus == Domain.Enumerations.PostingStatus.Posted)
        {
            _logger.LogInformation(
                "Reserve {Id} already posted, skipping", reserveHistoryId);
            return;
        }

        // Simulate GL posting
        history.PostingStatus = Domain.Enumerations.PostingStatus.Posted;
        await _context.SaveChangesAsync(cancellationToken);

        await _auditLog.LogAsync(
            claimId: history.ClaimId,
            organisationId: history.OrganisationId,
            eventType: AuditEventTypes.GlPostingSimulated,
            description: $"DR Change in Outstanding Reserves / CR Outstanding Loss Reserves, Amount = {history.Amount:N2}",
            relatedEntityId: reserveHistoryId,
            relatedEntityType: "ReserveHistory",
            cancellationToken: cancellationToken);

        _logger.LogInformation(
            "GL posting completed for reserve {ReserveHistoryId}", reserveHistoryId);
    }
}