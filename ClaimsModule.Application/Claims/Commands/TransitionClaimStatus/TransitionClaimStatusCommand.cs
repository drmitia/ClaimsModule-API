using ClaimsModule.Domain.Enumerations;
using MediatR;
using ClaimsModule.Application.Common.Models;

namespace ClaimsModule.Application.Claims.Commands.TransitionClaimStatus;

public record TransitionClaimStatusCommand : IRequest<TransitionResultDto>
{
    public Guid ClaimId { get; init; }
    public ClaimStatus NewStatus { get; init; }
    public string? Reason { get; init; }

    // Required when closing with open reserves (CC-04)
    public bool ConfirmCloseWithOpenReserves { get; init; } = false;
    public string? OpenReservesJustification { get; init; }
}