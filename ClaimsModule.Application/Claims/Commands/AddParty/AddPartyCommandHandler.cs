using ClaimsModule.Application.Common;
using ClaimsModule.Application.Common.Interfaces;
using ClaimsModule.Domain.Entities;
using ClaimsModule.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClaimsModule.Application.Claims.Commands.AddParty;

public class AddPartyCommandHandler : IRequestHandler<AddPartyCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuditLogService _auditLog;
    private readonly ICorrelationIdService _correlationId;

    public AddPartyCommandHandler(
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
        AddPartyCommand request,
        CancellationToken cancellationToken)
    {
        var claim = await _context.Claims
            .FirstOrDefaultAsync(c =>
                c.Id == request.ClaimId &&
                c.OrganisationId == _currentUser.OrganisationId &&
                !c.IsDeleted, cancellationToken)
            ?? throw new ClaimNotFoundException(request.ClaimId);

        var party = new ClaimParty
        {
            ClaimId = claim.Id,
            PartyRole = request.PartyRole,
            PartyType = request.PartyType,
            FirstName = request.FirstName,
            LastName = request.LastName,
            CompanyName = request.CompanyName,
            Email = request.Email,
            Phone = request.Phone,
            Notes = request.Notes,
            IsActive = true,
            OrganisationId = _currentUser.OrganisationId,
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedByUserId = _currentUser.UserId
        };

        _context.ClaimParties.Add(party);
        await _context.SaveChangesAsync(cancellationToken);

        await _auditLog.LogAsync(
            claimId: claim.Id,
            organisationId: _currentUser.OrganisationId,
            eventType: AuditEventTypes.PartyAdded,
            description: $"Party added: {party.DisplayName} as {request.PartyRole}",
            createdByUserId: _currentUser.UserId,
            relatedEntityId: party.Id,
            relatedEntityType: "ClaimParty",
            correlationId: _correlationId.CorrelationId,
            cancellationToken: cancellationToken);

        return party.Id;
    }
}