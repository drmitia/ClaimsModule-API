using ClaimsModule.Application.Common.Models;
using ClaimsModule.Domain.Enumerations;
using MediatR;

namespace ClaimsModule.Application.Claims.Queries.ListClaims;

public record ListClaimsQuery : IRequest<PaginatedList<ClaimSummaryDto>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public ClaimStatus? Status { get; init; }
    public string? SearchTerm { get; init; }
    public Guid? AssignedHandlerId { get; init; }
}