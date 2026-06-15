using ClaimsModule.Application.Common;
using ClaimsModule.Application.Common.Interfaces;
using ClaimsModule.Application.Common.Models;
using ClaimsModule.Domain.Enumerations;
using ClaimsModule.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClaimsModule.Application.Claims.Commands.TransitionClaimStatus;

public class TransitionClaimStatusHandler
    : IRequestHandler<TransitionClaimStatusCommand, TransitionResultDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuditLogService _auditLog;
    private readonly ICorrelationIdService _correlationId;

    public TransitionClaimStatusHandler(
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

    public async Task<TransitionResultDto> Handle(
        TransitionClaimStatusCommand request,
        CancellationToken cancellationToken)
    {
        var claim = await _context.Claims
            .Include(c => c.Parties.Where(p => p.IsActive && !p.IsDeleted))
            .Include(c => c.ReserveComponents)
                .ThenInclude(r => r.History)
            .FirstOrDefaultAsync(c =>
                c.Id == request.ClaimId &&
                c.OrganisationId == _currentUser.OrganisationId &&
                !c.IsDeleted, cancellationToken)
            ?? throw new ClaimNotFoundException(request.ClaimId);

        // Validate the transition is allowed by state machine
        ClaimStatusStateMachine.ValidateTransition(claim.Status, request.NewStatus);

        var result = new TransitionResultDto { Succeeded = true };

        // Draft -> Open checks
        if (claim.Status == ClaimStatus.Draft && request.NewStatus == ClaimStatus.Open)
        {
            var hasClaimant = claim.Parties
                .Any(p => p.PartyRole == PartyRole.Claimant);

            if (!hasClaimant)
                result.BlockingIssues.Add(
                    "At least one party with role 'Claimant' is required before opening a claim.");
        }

        // Closure checks (CC-01 to CC-04)
        if (request.NewStatus == ClaimStatus.Closed)
        {
            // CC-01 — No pending reserves
            var hasPendingReserves = claim.ReserveComponents
                .Any(rc => rc.History
                    .Any(h => h.ApprovalStatus == ApprovalStatus.PendingApproval));

            if (hasPendingReserves)
                result.BlockingIssues.Add(
                    "CC-01: Cannot close claim while reserve components have pending approval.");

            // CC-02 — No unresolved critical issues
            // Reserved for future validation issue tracking
            // Currently no critical issues mechanism implemented beyond this check

            // CC-03 — At least one Claimant
            var hasClaimant = claim.Parties
                .Any(p => p.PartyRole == PartyRole.Claimant);

            if (!hasClaimant)
                result.BlockingIssues.Add(
                    "CC-03: At least one party with role 'Claimant' is required to close a claim.");

            // CC-04 — Warning if open reserves exist
            var hasOpenReserves = claim.ReserveComponents
                .Any(rc => rc.CurrentAmount > 0);

            if (hasOpenReserves)
            {
                if (!request.ConfirmCloseWithOpenReserves)
                {
                    result.Warnings.Add(
                        "CC-04: Claim has reserve components with a balance greater than zero. " +
                        "Set 'confirmCloseWithOpenReserves' to true and provide a justification note to proceed.");
                    result.Succeeded = false;
                    return result;
                }

                if (string.IsNullOrWhiteSpace(request.OpenReservesJustification))
                {
                    result.BlockingIssues.Add(
                        "CC-04: A justification note is required when closing a claim with open reserves.");
                }
            }
        }

        // If any blocking issues return without saving
        if (result.BlockingIssues.Any())
        {
            result.Succeeded = false;
            return result;
        }

        // All checks passed — perform the transition
        var previousStatus = claim.Status;
        claim.Status = request.NewStatus;
        claim.UpdatedAt = DateTimeOffset.UtcNow;
        claim.ModifiedByUserId = _currentUser.UserId;

        if (request.NewStatus == ClaimStatus.Closed)
        {
            claim.ClosedAt = DateTimeOffset.UtcNow;
            claim.ClosureReason = request.Reason;

            if (!string.IsNullOrWhiteSpace(request.OpenReservesJustification))
                claim.Notes = string.IsNullOrWhiteSpace(claim.Notes)
                    ? $"Closure justification: {request.OpenReservesJustification}"
                    : claim.Notes + $"\nClosure justification: {request.OpenReservesJustification}";
        }

        await _context.SaveChangesAsync(cancellationToken);

        await _auditLog.LogAsync(
            claimId: claim.Id,
            organisationId: _currentUser.OrganisationId,
            eventType: AuditEventTypes.ClaimStatusChanged,
            description: "Status changed from " + previousStatus.ToString() + " to " + request.NewStatus.ToString(),
            createdByUserId: _currentUser.UserId,
            oldValue: previousStatus.ToString(),
            newValue: request.NewStatus.ToString(),
            correlationId: _correlationId.CorrelationId,
            cancellationToken: cancellationToken);

        return result;
    }
}