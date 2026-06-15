using ClaimsModule.Domain.Enumerations;
using MediatR;

namespace ClaimsModule.Application.Reserves.Commands.CreateReserve;

public record CreateReserveCommand : IRequest<Guid>
{
    public Guid ClaimId { get; init; }
    public ReserveComponent Component { get; init; }
    public decimal Amount { get; init; }
    public string ChangeReason { get; init; } = string.Empty;
}