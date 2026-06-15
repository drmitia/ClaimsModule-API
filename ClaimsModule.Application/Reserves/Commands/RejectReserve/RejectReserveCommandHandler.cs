using ClaimsModule.Application.Common;
using ClaimsModule.Application.Common.Interfaces;
using ClaimsModule.Domain.Enumerations;
using ClaimsModule.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClaimsModule.Application.Reserves.Commands.RejectReserve;

public class RejectReserveCommandHandler : IRequestHandler<RejectReserveCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuditLogService _auditLog;
    private readonly ICorrelationIdService _correlationId;

    public RejectReserveCommandHandler(
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
        RejectReserveCommand request,
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
                $"Reserve is not pending approval. Current status: {history.ApprovalStatus}");

        // Self rejection check
        if (history.SubmittedByUserId == _currentUser.UserId)
            throw new SelfApprovalException();

        // Role check — handlers cannot reject
        if (_currentUser.Role == UserRole.Handler)
            throw new InsufficientAuthorityException(
                "Handlers cannot reject reserves.");

        history.ApprovalStatus = ApprovalStatus.Rejected;
        history.RejectedByUserId = _currentUser.UserId;
        history.RejectedAt = DateTimeOffset.UtcNow;
        history.RejectionReason = request.RejectionReason;

        await _context.SaveChangesAsync(cancellationToken);

        await _auditLog.LogAsync(
            claimId: request.ClaimId,
            organisationId: _currentUser.OrganisationId,
            eventType: AuditEventTypes.ReserveRejected,
            description: $"Reserve of ${history.Amount:N2} rejected. Reason: {request.RejectionReason}",
            createdByUserId: _currentUser.UserId,
            relatedEntityId: history.Id,
            relatedEntityType: "ReserveHistory",
            correlationId: _correlationId.CorrelationId,
            cancellationToken: cancellationToken);
    }
}