using MediatR;

namespace ClaimsModule.Application.Claims.Commands.RemoveParty;

public record RemovePartyCommand : IRequest
{
    public Guid ClaimId { get; init; }
    public Guid PartyId { get; init; }
}