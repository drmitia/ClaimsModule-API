using ClaimsModule.Domain.Common;
using ClaimsModule.Domain.Enumerations;

namespace ClaimsModule.Domain.Entities;

public class ClaimRiskObject : BaseEntity
{
    public Guid ClaimId { get; set; }
    public AssetType AssetType { get; set; }
    public string AssetDescription { get; set; } = string.Empty;
    public string? DamageDescription { get; set; }
    public bool IsPrimary { get; set; } = false;
    public string? AssetReference { get; set; }

    // Navigation
    public Claim? Claim { get; set; }
}