using ClaimsModule.Domain.Common;
using ClaimsModule.Domain.Enumerations;

namespace ClaimsModule.Domain.Entities;

public class Claim : BaseEntity
{
    public string ClaimNumber { get; set; } = string.Empty;
    public Guid? PolicyId { get; set; }
    public string? PolicyNumber { get; set; }
    public string? ClientName { get; set; }
    public ClaimStatus Status { get; set; } = ClaimStatus.Draft;
    public ClaimSeverity? Severity { get; set; }
    public DateTimeOffset ReportedDate { get; set; }
    public Guid? AssignedHandlerId { get; set; }
    public DateTimeOffset? ClosedAt { get; set; }
    public string? ClosureReason { get; set; }
    public string? Notes { get; set; }
    public bool AggregateOverrideFlag { get; set; } = false;

    // Optimistic concurrency token
    public byte[] RowVer { get; set; } = Array.Empty<byte>();

    // Navigation properties
    public LossEvent? LossEvent { get; set; }
    public ICollection<ClaimParty> Parties { get; set; } = new List<ClaimParty>();
    public ICollection<ClaimRiskObject> RiskObjects { get; set; } = new List<ClaimRiskObject>();
    public ICollection<ClaimReserveComponent> ReserveComponents { get; set; } = new List<ClaimReserveComponent>();
    public ICollection<ClaimDocument> Documents { get; set; } = new List<ClaimDocument>();
    public ICollection<ClaimAuditLog> AuditLogs { get; set; } = new List<ClaimAuditLog>();
}