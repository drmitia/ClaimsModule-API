using MediatR;

namespace ClaimsModule.Application.Reserves.Commands.ApproveReserve;

public record ApproveReserveCommand : IRequest
{
    public Guid ReserveHistoryId { get; init; }
    public Guid ClaimId { get; init; }
}