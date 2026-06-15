using ClaimsModule.Domain.Enumerations;
using ClaimsModule.Domain.Exceptions;

namespace ClaimsModule.Application.Claims.Commands.TransitionClaimStatus;

public static class ClaimStatusStateMachine
{
    private static readonly Dictionary<ClaimStatus, ClaimStatus[]> _allowedTransitions = new()
    {
        [ClaimStatus.Draft]               = [ClaimStatus.Open, ClaimStatus.Withdrawn],
        [ClaimStatus.Open]                = [ClaimStatus.UnderInvestigation, ClaimStatus.PendingPayment, ClaimStatus.Closed, ClaimStatus.Withdrawn],
        [ClaimStatus.UnderInvestigation]  = [ClaimStatus.Open, ClaimStatus.PendingPayment, ClaimStatus.Closed],
        [ClaimStatus.PendingPayment]      = [ClaimStatus.Closed, ClaimStatus.UnderInvestigation],
        [ClaimStatus.Closed]              = [ClaimStatus.Reopened],
        [ClaimStatus.Reopened]            = [ClaimStatus.Open, ClaimStatus.Closed, ClaimStatus.Withdrawn],
        [ClaimStatus.Withdrawn]           = []
    };

    public static void ValidateTransition(ClaimStatus from, ClaimStatus to)
    {
        if (!_allowedTransitions.TryGetValue(from, out var allowed) || !allowed.Contains(to))
            throw new InvalidStatusTransitionException(from.ToString(), to.ToString());
    }
}