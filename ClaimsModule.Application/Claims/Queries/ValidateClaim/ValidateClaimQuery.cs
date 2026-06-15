using ClaimsModule.Application.Common.Models;
using MediatR;

namespace ClaimsModule.Application.Claims.Queries.ValidateClaim;

public record ValidateClaimQuery(Guid ClaimId) : IRequest<ValidationResultDto>;