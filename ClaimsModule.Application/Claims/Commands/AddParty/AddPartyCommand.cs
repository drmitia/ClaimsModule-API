using ClaimsModule.Domain.Enumerations;
using MediatR;

namespace ClaimsModule.Application.Claims.Commands.AddParty;

public record AddPartyCommand : IRequest<Guid>
{
    public Guid ClaimId { get; init; }
    public PartyRole PartyRole { get; init; }
    public PartyType PartyType { get; init; }
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public string? CompanyName { get; init; }
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public string? Notes { get; init; }
}