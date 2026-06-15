using MediatR;

namespace ClaimsModule.Application.Claims.Commands.RemoveRiskObject;

public record RemoveRiskObjectCommand : IRequest
{
    public Guid ClaimId { get; init; }
    public Guid RiskObjectId { get; init; }
}