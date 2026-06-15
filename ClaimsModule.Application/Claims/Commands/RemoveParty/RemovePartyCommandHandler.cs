using ClaimsModule.Application.Common;
using ClaimsModule.Application.Common.Interfaces;
using ClaimsModule.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClaimsModule.Application.Claims.Commands.RemoveParty;

public class RemovePartyCommandHandler : IRequestHandler<RemovePartyCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuditLogService _auditLog;
    private readonly ICorrelationIdService _correlationId;

    public RemovePartyCommandHandler(
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
        RemovePartyCommand request,
        CancellationToken cancellationToken)
    {
        var party = await _context.ClaimParties
            .FirstOrDefaultAsync(p =>
                p.Id == request.PartyId &&
                p.ClaimId == request.ClaimId &&
                p.OrganisationId == _currentUser.OrganisationId &&
                !p.IsDeleted, cancellationToken)
            ?? throw new DomainException(
                $"Party with ID '{request.PartyId}' was not found.");

        // Soft delete
        party.IsActive = false;
        party.IsDeleted = true;
        party.DeletedAt = DateTimeOffset.UtcNow;
        party.UpdatedAt = DateTimeOffset.UtcNow;
        party.ModifiedByUserId = _currentUser.UserId;

        await _context.SaveChangesAsync(cancellationToken);

        await _auditLog.LogAsync(
            claimId: request.ClaimId,
            organisationId: _currentUser.OrganisationId,
            eventType: AuditEventTypes.PartyRemoved,
            description: $"Party {party.DisplayName} removed from claim",
            createdByUserId: _currentUser.UserId,
            relatedEntityId: party.Id,
            relatedEntityType: "ClaimParty",
            correlationId: _correlationId.CorrelationId,
            cancellationToken: cancellationToken);
    }
}