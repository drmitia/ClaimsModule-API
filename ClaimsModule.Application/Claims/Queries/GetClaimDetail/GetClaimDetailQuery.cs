using ClaimsModule.Application.Common.Models;
using MediatR;

namespace ClaimsModule.Application.Claims.Queries.GetClaimDetail;

public record GetClaimDetailQuery(Guid ClaimId) : IRequest<ClaimDetailDto>;