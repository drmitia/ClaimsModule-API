using ClaimsModule.Application.Common;
using ClaimsModule.Application.Common.Interfaces;
using ClaimsModule.Domain.Enumerations;
using ClaimsModule.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClaimsModule.Application.Reserves.Commands.RetractReserve;

public class RetractReserveCommandHandler : IRequestHandler<RetractReserveCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuditLogService _auditLog;
    private readonly ICorrelationIdService _correlationId;

    public RetractReserveCommandHandler(
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
        RetractReserveCommand request,
        CancellationToken cancellationToken)
    {
        var history = await _context.ReserveHistories
            .FirstOrDefaultAsync(h =>
                h.Id == request.ReserveHistoryId &&
                h.ClaimId == request.ClaimId,
                cancellationToken)
            ?? throw new ReserveNotFoundException(request.ReserveHistoryId);

        if (history.ApprovalStatus != ApprovalStatus.PendingApproval)
            throw new DomainException(
                $"Only pending reserves can be retracted. Current status: {history.ApprovalStatus}");

        // Only the submitter can retract
        if (history.SubmittedByUserId != _currentUser.UserId)
            throw new DomainException(
                "Only the original submitter can retract a reserve.");

        history.ApprovalStatus = ApprovalStatus.Cancelled;
        await _context.SaveChangesAsync(cancellationToken);

        await _auditLog.LogAsync(
            claimId: request.ClaimId,
            organisationId: _currentUser.OrganisationId,
            eventType: AuditEventTypes.ReserveRetracted,
            description: $"Reserve of ${history.Amount:N2} retracted by submitter",
            createdByUserId: _currentUser.UserId,
            relatedEntityId: history.Id,
            relatedEntityType: "ReserveHistory",
            correlationId: _correlationId.CorrelationId,
            cancellationToken: cancellationToken);
    }
}