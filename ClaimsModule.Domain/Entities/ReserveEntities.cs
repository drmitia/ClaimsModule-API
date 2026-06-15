using ClaimsModule.Domain.Common;
using ClaimsModule.Domain.Enumerations;

namespace ClaimsModule.Domain.Entities;

public class ClaimReserveComponent : BaseEntity
{
    public Guid ClaimId { get; set; }
    public ReserveComponent Component { get; set; }
    public decimal CurrentAmount { get; set; } = 0;
    public string? Notes { get; set; }

    public byte[] RowVer { get; set; } = Array.Empty<byte>();

    // Navigation
    public Claim? Claim { get; set; }
    public ICollection<ReserveHistory> History { get; set; } = new List<ReserveHistory>();
}

public class ReserveHistory : BaseEntity
{
    public Guid ReserveComponentId { get; set; }
    public Guid ClaimId { get; set; }
    public TransactionType TransactionType { get; set; }
    public decimal Amount { get; set; }
    public decimal PreviousBalance { get; set; }
    public decimal NewBalance { get; set; }
    public ApprovalStatus ApprovalStatus { get; set; }
    public Guid? ApprovedByUserId { get; set; }
    public DateTimeOffset? ApprovedAt { get; set; }
    public Guid? RejectedByUserId { get; set; }
    public DateTimeOffset? RejectedAt { get; set; }
    public string? RejectionReason { get; set; }
    public string ChangeReason { get; set; } = string.Empty;
    public PostingStatus PostingStatus { get; set; } = PostingStatus.Pending;
    public string? PostingJobId { get; set; }
    public string IdempotencyKey { get; set; } = string.Empty;
    public int ChangeSequence { get; set; }
    public Guid? SubmittedByUserId { get; set; }

    // Navigation
    public ClaimReserveComponent? ReserveComponent { get; set; }
}