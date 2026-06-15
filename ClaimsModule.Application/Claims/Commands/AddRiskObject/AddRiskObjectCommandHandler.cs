using ClaimsModule.Application.Common;
using ClaimsModule.Application.Common.Interfaces;
using ClaimsModule.Domain.Entities;
using ClaimsModule.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClaimsModule.Application.Claims.Commands.AddRiskObject;

public class AddRiskObjectCommandHandler : IRequestHandler<AddRiskObjectCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuditLogService _auditLog;
    private readonly ICorrelationIdService _correlationId;

    public AddRiskObjectCommandHandler(
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

    public async Task<Guid> Handle(
        AddRiskObjectCommand request,
        CancellationToken cancellationToken)
    {
        var claim = await _context.Claims
            .FirstOrDefaultAsync(c =>
                c.Id == request.ClaimId &&
                c.OrganisationId == _currentUser.OrganisationId &&
                !c.IsDeleted, cancellationToken)
            ?? throw new ClaimNotFoundException(request.ClaimId);

        // If this is marked primary, unmark any existing primary
        if (request.IsPrimary)
        {
            var existingPrimary = await _context.ClaimRiskObjects
                .Where(r => r.ClaimId == claim.Id && r.IsPrimary && !r.IsDeleted)
                .ToListAsync(cancellationToken);

            foreach (var existing in existingPrimary)
            {
                existing.IsPrimary = false;
                existing.UpdatedAt = DateTimeOffset.UtcNow;
            }
        }

        var riskObject = new ClaimRiskObject
        {
            ClaimId = claim.Id,
            AssetType = request.AssetType,
            AssetDescription = request.AssetDescription,
            DamageDescription = request.DamageDescription,
            IsPrimary = request.IsPrimary,
            AssetReference = request.AssetReference,
            OrganisationId = _currentUser.OrganisationId,
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedByUserId = _currentUser.UserId
        };

        _context.ClaimRiskObjects.Add(riskObject);
        await _context.SaveChangesAsync(cancellationToken);

        await _auditLog.LogAsync(
            claimId: claim.Id,
            organisationId: _currentUser.OrganisationId,
            eventType: AuditEventTypes.RiskObjectAdded,
            description: $"Risk object added: {request.AssetType} - {request.AssetDescription}",
            createdByUserId: _currentUser.UserId,
            relatedEntityId: riskObject.Id,
            relatedEntityType: "ClaimRiskObject",
            correlationId: _correlationId.CorrelationId,
            cancellationToken: cancellationToken);

        return riskObject.Id;
    }
}