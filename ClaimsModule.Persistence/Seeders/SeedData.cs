using ClaimsModule.Domain.Entities;
using ClaimsModule.Domain.Enumerations;

namespace ClaimsModule.Persistence.Seeders;

public static class SeedData
{
    public static readonly CauseOfLossCode[] CauseOfLossCodes =
    [
        new() { CauseOfLossCodeId = Guid.Parse("10000000-0000-0000-0000-000000000001"), Code = "FIRE", Name = "Fire", PerilCategory = "Property", IsActive = true, SortOrder = 1 },
        new() { CauseOfLossCodeId = Guid.Parse("10000000-0000-0000-0000-000000000002"), Code = "FLOOD", Name = "Flood", PerilCategory = "Property", IsActive = true, SortOrder = 2 },
        new() { CauseOfLossCodeId = Guid.Parse("10000000-0000-0000-0000-000000000003"), Code = "THEFT", Name = "Theft", PerilCategory = "Crime", IsActive = true, SortOrder = 3 },
        new() { CauseOfLossCodeId = Guid.Parse("10000000-0000-0000-0000-000000000004"), Code = "COLLISION", Name = "Vehicle Collision", PerilCategory = "Motor", IsActive = true, SortOrder = 4 },
        new() { CauseOfLossCodeId = Guid.Parse("10000000-0000-0000-0000-000000000005"), Code = "WIND", Name = "Wind / Storm", PerilCategory = "Property", IsActive = true, SortOrder = 5 },
        new() { CauseOfLossCodeId = Guid.Parse("10000000-0000-0000-0000-000000000006"), Code = "LIAB", Name = "Liability", PerilCategory = "Liability", IsActive = true, SortOrder = 6 },
        new() { CauseOfLossCodeId = Guid.Parse("10000000-0000-0000-0000-000000000007"), Code = "MEDPAY", Name = "Medical Payment", PerilCategory = "Health", IsActive = true, SortOrder = 7 },
        new() { CauseOfLossCodeId = Guid.Parse("10000000-0000-0000-0000-000000000008"), Code = "VANDAL", Name = "Vandalism", PerilCategory = "Crime", IsActive = true, SortOrder = 8 },
        new() { CauseOfLossCodeId = Guid.Parse("10000000-0000-0000-0000-000000000009"), Code = "SLIP", Name = "Slip and Fall", PerilCategory = "Liability", IsActive = true, SortOrder = 9 },
        new() { CauseOfLossCodeId = Guid.Parse("10000000-0000-0000-0000-000000000010"), Code = "WATER", Name = "Water Damage", PerilCategory = "Property", IsActive = true, SortOrder = 10 },
    ];

    public static readonly Policy[] Policies =
    [
        new() { PolicyId = Guid.Parse("20000000-0000-0000-0000-000000000001"), PolicyNumber = "POL-2024-00001", ClientName = "Acme Corporation", EffectiveDate = new DateOnly(2024, 1, 1), ExpirationDate = new DateOnly(2025, 12, 31), Status = PolicyStatus.Active, CoverageTypes = "Property, Liability" },
        new() { PolicyId = Guid.Parse("20000000-0000-0000-0000-000000000002"), PolicyNumber = "POL-2024-00002", ClientName = "GlobalTech Ltd", EffectiveDate = new DateOnly(2024, 3, 1), ExpirationDate = new DateOnly(2025, 2, 28), Status = PolicyStatus.Active, CoverageTypes = "Motor, Liability" },
        new() { PolicyId = Guid.Parse("20000000-0000-0000-0000-000000000003"), PolicyNumber = "POL-2024-00003", ClientName = "Smith & Partners", EffectiveDate = new DateOnly(2023, 6, 1), ExpirationDate = new DateOnly(2024, 5, 31), Status = PolicyStatus.Expired, CoverageTypes = "Property" },
        new() { PolicyId = Guid.Parse("20000000-0000-0000-0000-000000000004"), PolicyNumber = "POL-2024-00004", ClientName = "Riverside Medical Group", EffectiveDate = new DateOnly(2024, 1, 1), ExpirationDate = new DateOnly(2026, 12, 31), Status = PolicyStatus.Active, CoverageTypes = "Health, Liability" },
        new() { PolicyId = Guid.Parse("20000000-0000-0000-0000-000000000005"), PolicyNumber = "POL-2024-00005", ClientName = "BlueSky Logistics", EffectiveDate = new DateOnly(2024, 7, 1), ExpirationDate = new DateOnly(2025, 6, 30), Status = PolicyStatus.Active, CoverageTypes = "Motor, Property, Liability" },
    ];
}