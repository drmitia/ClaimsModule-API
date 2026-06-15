using ClaimsModule.Application.Common;
using ClaimsModule.Application.Common.Interfaces;
using ClaimsModule.Domain.Entities;
using ClaimsModule.Domain.Enumerations;
using ClaimsModule.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClaimsModule.Application.Reserves.Commands.CreateReserve;

public class CreateReserveCommandHandler : IRequestHandler<CreateReserveCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuditLogService _auditLog;
    private readonly ICorrelationIdService _correlationId;

    private const decimal AutoApprovalLimit = 10_000m;
    private const decimal SupervisorLimit = 100_000m;

    public CreateReserveCommandHandler(
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
        CreateReserveCommand request,
        CancellationToken cancellationToken)
    {
        var claim = await _context.Claims
            .Include(c => c.ReserveComponents)
                .ThenInclude(rc => rc.History)
            .FirstOrDefaultAsync(c =>
                c.Id == request.ClaimId &&
                c.OrganisationId == _currentUser.OrganisationId &&
                !c.IsDeleted, cancellationToken)
            ?? throw new ClaimNotFoundException(request.ClaimId);

        // Check aggregate cap
        var totalReserves = claim.ReserveComponents.Sum(r => r.CurrentAmount);
        if (totalReserves + request.Amount > 10_000_000m && !claim.AggregateOverrideFlag)
            throw new InsufficientAuthorityException(
                "This reserve would exceed the $10,000,000 aggregate cap. " +
                "A manager must set the override flag.");

        // Find or create reserve component
        var reserveComponent = claim.ReserveComponents
            .FirstOrDefault(r => r.Component == request.Component);

        var isNew = reserveComponent is null;

        if (isNew)
        {
            reserveComponent = new ClaimReserveComponent
            {
                ClaimId = claim.Id,
                Component = request.Component,
                CurrentAmount = 0,
                OrganisationId = _currentUser.OrganisationId,
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedByUserId = _currentUser.UserId
            };

            _context.ClaimReserveComponents.Add(reserveComponent);

            // Save first so database assigns RowVer
            await _context.SaveChangesAsync(cancellationToken);
        }

        // Determine approval status based on authority
        var approvalStatus = DetermineApprovalStatus(request.Amount);

        // Get next sequence number
        var nextSequence = reserveComponent!.History.Count + 1;

        var previousBalance = reserveComponent.CurrentAmount;
        var newBalance = approvalStatus == ApprovalStatus.AutoApproved
            ? previousBalance + request.Amount
            : previousBalance;

        // Create history entry
        var history = new ReserveHistory
        {
            ClaimId = claim.Id,
            ReserveComponentId = reserveComponent.Id,
            TransactionType = TransactionType.Add,
            Amount = request.Amount,
            PreviousBalance = previousBalance,
            NewBalance = newBalance,
            ApprovalStatus = approvalStatus,
            ChangeReason = request.ChangeReason,
            ChangeSequence = nextSequence,
            IdempotencyKey = $"Reserve:{reserveComponent.Id}:Change:{nextSequence}",
            SubmittedByUserId = _currentUser.UserId,
            OrganisationId = _currentUser.OrganisationId,
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedByUserId = _currentUser.UserId
        };

        // If auto approved update balance immediately
        if (approvalStatus == ApprovalStatus.AutoApproved)
        {
            reserveComponent.CurrentAmount = newBalance;
            history.ApprovedByUserId = _currentUser.UserId;
            history.ApprovedAt = DateTimeOffset.UtcNow;
            history.PostingStatus = PostingStatus.Pending;
        }

        // Save history separately after component exists
        _context.ReserveHistories.Add(history);
        await _context.SaveChangesAsync(cancellationToken);

        // Audit log
        var eventType = approvalStatus == ApprovalStatus.AutoApproved
            ? AuditEventTypes.ReserveAutoApproved
            : AuditEventTypes.ReservePendingApproval;

        await _auditLog.LogAsync(
            claimId: claim.Id,
            organisationId: _currentUser.OrganisationId,
            eventType: eventType,
            description: $"Reserve of ${request.Amount:N2} for " +
                         request.Component.ToString() +
                         (approvalStatus == ApprovalStatus.AutoApproved
                             ? " auto-approved"
                             : " pending approval"),
            createdByUserId: _currentUser.UserId,
            correlationId: _correlationId.CorrelationId,
            relatedEntityId: history.Id,
            relatedEntityType: "ReserveHistory",
            cancellationToken: cancellationToken);

        return history.Id;
    }

    private ApprovalStatus DetermineApprovalStatus(decimal amount)
    {
        if (amount <= AutoApprovalLimit)
            return ApprovalStatus.AutoApproved;

        if (amount <= SupervisorLimit)
        {
            if (_currentUser.Role is UserRole.Supervisor or UserRole.Manager)
                return ApprovalStatus.AutoApproved;
            return ApprovalStatus.PendingApproval;
        }

        if (_currentUser.Role == UserRole.Manager)
            return ApprovalStatus.AutoApproved;

        return ApprovalStatus.PendingApproval;
    }
}