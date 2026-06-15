using ClaimsModule.Domain.Enumerations;
using MediatR;

namespace ClaimsModule.Application.Claims.Commands.AddRiskObject;

public record AddRiskObjectCommand : IRequest<Guid>
{
    public Guid ClaimId { get; init; }
    public AssetType AssetType { get; init; }
    public string AssetDescription { get; init; } = string.Empty;
    public string? DamageDescription { get; init; }
    public bool IsPrimary { get; init; } = false;
    public string? AssetReference { get; init; }
}