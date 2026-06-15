using ClaimsModule.Application.Common;
using ClaimsModule.Application.Common.Interfaces;
using ClaimsModule.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClaimsModule.Application.Claims.Commands.RemoveRiskObject;

public class RemoveRiskObjectCommandHandler : IRequestHandler<RemoveRiskObjectCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuditLogService _auditLog;
    private readonly ICorrelationIdService _correlationId;

    public RemoveRiskObjectCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        IAuditLogService auditLog,
        ICorrelationIdService correlationId)
    {
        _context = context;
        _currentUser = currentUser;
        _auditLog = auditLog;
        _correlationId = correlationId;
    }

    public async Task Handle(
        RemoveRiskObjectCommand request,
        CancellationToken cancellationToken)
    {
        var riskObject = await _context.ClaimRiskObjects
            .FirstOrDefaultAsync(r =>
                r.Id == request.RiskObjectId &&
                r.ClaimId == request.ClaimId &&
                r.OrganisationId == _currentUser.OrganisationId &&
                !r.IsDeleted, cancellationToken)
            ?? throw new DomainException(
                $"Risk object with ID '{request.RiskObjectId}' was not found.");

        riskObject.IsDeleted = true;
        riskObject.DeletedAt = DateTimeOffset.UtcNow;
        riskObject.UpdatedAt = DateTimeOffset.UtcNow;
        riskObject.ModifiedByUserId = _currentUser.UserId;

        await _context.SaveChangesAsync(cancellationToken);

        await _auditLog.LogAsync(
            claimId: request.ClaimId,
            organisationId: _currentUser.OrganisationId,
            eventType: AuditEventTypes.RiskObjectRemoved,
            description: $"Risk object '{riskObject.AssetDescription}' removed from claim",
            createdByUserId: _currentUser.UserId,
            relatedEntityId: riskObject.Id,
            relatedEntityType: "ClaimRiskObject",
            correlationId: _correlationId.CorrelationId,
            cancellationToken: cancellationToken);
    }
}