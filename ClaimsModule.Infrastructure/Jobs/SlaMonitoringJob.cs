using ClaimsModule.Application.Common.Interfaces;
using ClaimsModule.Application.Common;
using ClaimsModule.Domain.Enumerations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClaimsModule.Infrastructure.Jobs;

public class SlaMonitoringJob
{
    private readonly IApplicationDbContext _context;
    private readonly IAuditLogService _auditLog;
    private readonly ILogger<SlaMonitoringJob> _logger;

    private static readonly ClaimStatus[] _monitoredStatuses =
        [ClaimStatus.Draft, ClaimStatus.Open];

    private const int SlaBreachHours = 48;

    public SlaMonitoringJob(
        IApplicationDbContext context,
        IAuditLogService auditLog,
        ILogger<SlaMonitoringJob> logger)
    {
        _context = context;
        _auditLog = auditLog;
        _logger = logger;
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("SLA monitoring job started at {Time}", DateTimeOffset.UtcNow);

        var breachThreshold = DateTimeOffset.UtcNow.AddHours(-SlaBreachHours);

        var breachedClaims = await _context.Claims
            .Where(c => _monitoredStatuses.Contains(c.Status)
                && c.UpdatedAt < breachThreshold
                && !c.IsDeleted)
            .ToListAsync(cancellationToken);

        _logger.LogInformation(
            "Found {Count} claims breaching SLA", breachedClaims.Count);

        foreach (var claim in breachedClaims)
        {
            await _auditLog.LogAsync(
                claimId: claim.Id,
                organisationId: claim.OrganisationId,
                eventType: AuditEventTypes.SlaBreachFlagged,
                description: $"Claim {claim.ClaimNumber} has been in '{claim.Status}' " +
                             $"status for more than {SlaBreachHours} hours",
                relatedEntityId: claim.Id,
                relatedEntityType: "Claim",
                cancellationToken: cancellationToken);
        }

        _logger.LogInformation("SLA monitoring job completed at {Time}", DateTimeOffset.UtcNow);
    }
}