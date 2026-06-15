using MediatR;

namespace ClaimsModule.Application.Reserves.Commands.RetractReserve;

public record RetractReserveCommand : IRequest
{
    public Guid ReserveHistoryId { get; init; }
    public Guid ClaimId { get; init; }
}