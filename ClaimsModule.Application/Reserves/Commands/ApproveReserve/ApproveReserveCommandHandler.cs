using ClaimsModule.Application.Common;
using ClaimsModule.Application.Common.Interfaces;
using ClaimsModule.Domain.Enumerations;
using ClaimsModule.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClaimsModule.Application.Reserves.Commands.ApproveReserve;

public class ApproveReserveCommandHandler : IRequestHandler<ApproveReserveCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuditLogService _auditLog;
    private readonly IBackgroundJobService _backgroundJobService;
    private readonly ICorrelationIdService _correlationId;

    private const decimal SupervisorLimit = 100_000m;

    public ApproveReserveCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        IAuditLogService auditLog,
        IBackgroundJobService backgroundJobService,
        ICorrelationIdService correlationId)
    {
        _context = context;
        _currentUser = currentUser;
        _auditLog = auditLog;
        _backgroundJobService = backgroundJobService;
        _correlationId = correlationId;
    }

    public async Task Handle(
        ApproveReserveCommand request,
        CancellationToken cancellationToken)
    {
        var history = await _context.ReserveHistories
            .Include(h => h.ReserveComponent)
            .FirstOrDefaultAsync(h =>
                h.Id == request.ReserveHistoryId &&
                h.ClaimId == request.ClaimId,
                cancellationToken)
            ?? throw new ReserveNotFoundException(request.ReserveHistoryId);

        // Must be pending
        if (history.ApprovalStatus != ApprovalStatus.PendingApproval)
            throw new DomainException(
                $"Reserve is not pending approval. Current status: {history.ApprovalStatus.ToString()}");

        // Self approval check
        if (history.SubmittedByUserId == _currentUser.UserId)
            throw new SelfApprovalException();

        // Authority check
        if (history.Amount > SupervisorLimit && _currentUser.Role != UserRole.Manager)
            throw new InsufficientAuthorityException(
                "Only a manager can approve reserves over $100,000.");

        if (history.Amount > 10_000m && _currentUser.Role == UserRole.Handler)
            throw new InsufficientAuthorityException(
                "Handlers cannot approve reserves over $10,000.");

        // Approve and update balance
        history.ApprovalStatus = ApprovalStatus.Approved;
        history.ApprovedByUserId = _currentUser.UserId;
        history.ApprovedAt = DateTimeOffset.UtcNow;

        var component = history.ReserveComponent!;
        component.CurrentAmount = component.CurrentAmount + history.Amount;
        history.NewBalance = component.CurrentAmount;

        await _context.SaveChangesAsync(cancellationToken);

        // Fire GL posting job
        _backgroundJobService.EnqueueGLPosting(history.Id, history.IdempotencyKey);

        await _auditLog.LogAsync(
            claimId: request.ClaimId,
            organisationId: _currentUser.OrganisationId,
            eventType: AuditEventTypes.ReserveApproved,
            description: $"Reserve of ${history.Amount:N2} approved",
            createdByUserId: _currentUser.UserId,
            relatedEntityId: history.Id,
            relatedEntityType: "ReserveHistory",
            correlationId: _correlationId.CorrelationId,
            cancellationToken: cancellationToken);
    }
}