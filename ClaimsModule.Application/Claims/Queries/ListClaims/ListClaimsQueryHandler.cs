using ClaimsModule.Application.Common.Interfaces;
using ClaimsModule.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClaimsModule.Application.Claims.Queries.ListClaims;

public class ListClaimsQueryHandler
    : IRequestHandler<ListClaimsQuery, PaginatedList<ClaimSummaryDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public ListClaimsQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<PaginatedList<ClaimSummaryDto>> Handle(
        ListClaimsQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.Claims
            .Include(c => c.LossEvent)
            .Include(c => c.ReserveComponents)
            .Where(c => c.OrganisationId == _currentUser.OrganisationId)
            .AsQueryable();

        // Filters
        if (request.Status.HasValue)
            query = query.Where(c => c.Status == request.Status.Value);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            query = query.Where(c =>
                c.ClaimNumber.Contains(request.SearchTerm) ||
                (c.ClientName != null && c.ClientName.Contains(request.SearchTerm)) ||
                (c.PolicyNumber != null && c.PolicyNumber.Contains(request.SearchTerm)));

        if (request.AssignedHandlerId.HasValue)
            query = query.Where(c => c.AssignedHandlerId == request.AssignedHandlerId);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(c => c.ReportedDate)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(c => new ClaimSummaryDto
            {
                Id = c.Id,
                ClaimNumber = c.ClaimNumber,
                PolicyNumber = c.PolicyNumber,
                ClientName = c.ClientName,
                Status = c.Status,
                Severity = c.Severity,
                ReportedDate = c.ReportedDate,
                AssignedHandlerId = c.AssignedHandlerId,
                CauseOfLossCode = c.LossEvent != null ? c.LossEvent.CauseOfLossCode : null,
                TotalReserve = c.ReserveComponents
                    .Sum(rc => rc.CurrentAmount),
                LossDate = c.LossEvent != null ? c.LossEvent.LossDate : null,
            })
            .ToListAsync(cancellationToken);

        return new PaginatedList<ClaimSummaryDto>(items, totalCount,
            request.PageNumber, request.PageSize);
    }
}