using ClaimsModule.Domain.Enumerations;
using MediatR;

namespace ClaimsModule.Application.Claims.Commands.CreateClaim;

public record CreateClaimCommand : IRequest<Guid>
{
    // Policy info
    public Guid? PolicyId { get; init; }
    public string? PolicyNumber { get; init; }
    public string? ClientName { get; init; }

    // Loss event
    public DateTimeOffset LossDate { get; init; }
    public string LossDescription { get; init; } = string.Empty;
    public string? LossLocation { get; init; }
    public string CauseOfLossCode { get; init; } = string.Empty;
    public decimal? EstimatedLossAmount { get; init; }
    public string? PoliceReportNumber { get; init; }

    // Optional initial reserve
    public decimal? InitialReserveAmount { get; init; }
    public ReserveComponent? InitialReserveComponent { get; init; }

    public string? Notes { get; init; }
}