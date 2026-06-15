using ClaimsModule.Application.Common.Interfaces;
using ClaimsModule.Application.Common.Models;
using ClaimsModule.Domain.Enumerations;
using ClaimsModule.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClaimsModule.Application.Claims.Queries.ValidateClaim;

public class ValidateClaimQueryHandler
    : IRequestHandler<ValidateClaimQuery, ValidationResultDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public ValidateClaimQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<ValidationResultDto> Handle(
        ValidateClaimQuery request,
        CancellationToken cancellationToken)
    {
        var claim = await _context.Claims
            .Include(c => c.Parties.Where(p => p.IsActive && !p.IsDeleted))
            .Include(c => c.RiskObjects.Where(r => !r.IsDeleted))
            .Include(c => c.LossEvent)
            .Include(c => c.ReserveComponents)
            .FirstOrDefaultAsync(c =>
                c.Id == request.ClaimId &&
                c.OrganisationId == _currentUser.OrganisationId,
                cancellationToken)
            ?? throw new ClaimNotFoundException(request.ClaimId);

        var result = new ValidationResultDto { IsValid = true };

        // Critical — no claimant party
        var hasClaimant = claim.Parties
            .Any(p => p.PartyRole == PartyRole.Claimant);

        if (!hasClaimant)
        {
            result.Issues.Add(new ValidationIssueDto
            {
                Code = "MISSING_CLAIMANT",
                Message = "At least one party with role 'Claimant' is required.",
                Severity = "Critical"
            });
        }

        // Critical — no loss event description
        if (claim.LossEvent is null ||
            string.IsNullOrWhiteSpace(claim.LossEvent.LossDescription))
        {
            result.Issues.Add(new ValidationIssueDto
            {
                Code = "MISSING_LOSS_DESCRIPTION",
                Message = "Loss description is required.",
                Severity = "Critical"
            });
        }

        // Warning — no policy linked
        if (claim.PolicyId is null)
        {
            result.Issues.Add(new ValidationIssueDto
            {
                Code = "POLICY_UNKNOWN",
                Message = "Warning: Policy Unknown — claim created without a linked policy.",
                Severity = "Warning"
            });
        }

        // Warning — no risk objects
        if (!claim.RiskObjects.Any())
        {
            result.Issues.Add(new ValidationIssueDto
            {
                Code = "NO_RISK_OBJECTS",
                Message = "No risk objects have been added to this claim.",
                Severity = "Warning"
            });
        }

        // Warning — pending reserves on closure attempt
        var hasPendingReserves = claim.ReserveComponents
            .Any(rc => rc.CurrentAmount > 0);

        if (hasPendingReserves && claim.Status == ClaimStatus.Open)
        {
            result.Issues.Add(new ValidationIssueDto
            {
                Code = "OPEN_RESERVES",
                Message = "Claim has reserve components with a balance greater than zero.",
                Severity = "Warning"
            });
        }

        // Mark invalid if any critical issues
        result.IsValid = result.Issues
            .All(i => i.Severity != "Critical");

        return result;
    }
}