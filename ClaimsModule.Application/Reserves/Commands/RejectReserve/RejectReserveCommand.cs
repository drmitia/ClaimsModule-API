using MediatR;

namespace ClaimsModule.Application.Reserves.Commands.RejectReserve;

public record RejectReserveCommand : IRequest
{
    public Guid ReserveHistoryId { get; init; }
    public Guid ClaimId { get; init; }
    public string RejectionReason { get; init; } = string.Empty;
}