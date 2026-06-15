using ClaimsModule.Domain.Enumerations;

namespace ClaimsModule.Application.Common.Models;

public class ClaimSummaryDto
{
    public Guid Id { get; set; }
    public string ClaimNumber { get; set; } = string.Empty;
    public string? PolicyNumber { get; set; }
    public string? ClientName { get; set; }
    public ClaimStatus Status { get; set; }
    public ClaimSeverity? Severity { get; set; }
    public DateTimeOffset ReportedDate { get; set; }
    public Guid? AssignedHandlerId { get; set; }
    public string? CauseOfLossCode { get; set; }  
    public decimal TotalReserve { get; set; } 
    public DateTimeOffset? LossDate { get; set; }
}

public class ClaimDetailDto
{
    public Guid Id { get; set; }
    public string ClaimNumber { get; set; } = string.Empty;
    public string? PolicyNumber { get; set; }
    public string? ClientName { get; set; }
    public ClaimStatus Status { get; set; }
    public ClaimSeverity? Severity { get; set; }
    public DateTimeOffset ReportedDate { get; set; }
    public Guid? AssignedHandlerId { get; set; }
    public string? Notes { get; set; }
    public DateTimeOffset? ClosedAt { get; set; }
    public string? ClosureReason { get; set; }

    public LossEventDto? LossEvent { get; set; }
    public List<ClaimPartyDto> Parties { get; set; } = new();
    public List<ClaimRiskObjectDto> RiskObjects { get; set; } = new();
    public List<ReserveComponentDto> ReserveComponents { get; set; } = new();
    public List<ClaimDocumentDto> Documents { get; set; } = new();
}

public class LossEventDto
{
    public Guid Id { get; set; }
    public DateTimeOffset LossDate { get; set; }
    public string LossDescription { get; set; } = string.Empty;
    public string? LossLocation { get; set; }
    public string CauseOfLossCode { get; set; } = string.Empty;
    public decimal? EstimatedLossAmount { get; set; }
    public string? PoliceReportNumber { get; set; }
}

public class ClaimPartyDto
{
    public Guid Id { get; set; }
    public PartyRole PartyRole { get; set; }
    public PartyType PartyType { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? CompanyName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string DisplayName { get; set; } = string.Empty;
}

public class ClaimRiskObjectDto
{
    public Guid Id { get; set; }
    public AssetType AssetType { get; set; }
    public string AssetDescription { get; set; } = string.Empty;
    public string? DamageDescription { get; set; }
    public bool IsPrimary { get; set; }
    public string? AssetReference { get; set; }
}

public class ReserveComponentDto
{
    public Guid Id { get; set; }
    public ReserveComponent Component { get; set; }
    public decimal CurrentAmount { get; set; }
    public string? Notes { get; set; }
    public List<ReserveHistoryDto> History { get; set; } = new();
}

public class ReserveHistoryDto
{
    public Guid Id { get; set; }
    public TransactionType TransactionType { get; set; }
    public decimal Amount { get; set; }
    public decimal PreviousBalance { get; set; }
    public decimal NewBalance { get; set; }
    public ApprovalStatus ApprovalStatus { get; set; }
    public string ChangeReason { get; set; } = string.Empty;
    public string? RejectionReason { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public Guid? SubmittedByUserId { get; set; }
    public Guid? ApprovedByUserId { get; set; }
}

public class ClaimDocumentDto
{
    public Guid Id { get; set; }
    public DocumentType DocumentType { get; set; }
    public string DocumentName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public DateTimeOffset UploadedAt { get; set; }
    public string? DownloadUrl { get; set; }
    public string? BlobPath { get; set; }
}

public class AuditLogDto
{
    public Guid AuditLogId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public Guid? CreatedByUserId { get; set; }
}

public class TransitionResultDto
{
    public bool Succeeded { get; set; }
    public List<string> BlockingIssues { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
}

public class ValidationResultDto
{
    public bool IsValid { get; set; }
    public List<ValidationIssueDto> Issues { get; set; } = new();
}

public class ValidationIssueDto
{
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty; // Critical or Warning
}