using ClaimsModule.Domain.Common;
using ClaimsModule.Domain.Enumerations;

namespace ClaimsModule.Domain.Entities;

public class ClaimDocument : BaseEntity
{
    public Guid ClaimId { get; set; }
    public DocumentType DocumentType { get; set; }
    public string DocumentName { get; set; } = string.Empty;
    public string BlobPath { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public DateTimeOffset UploadedAt { get; set; }
    public Guid? UploadedByUserId { get; set; }
    public string? Notes { get; set; }

    // Navigation
    public Claim? Claim { get; set; }
}

public class ClaimAuditLog
{
    public Guid AuditLogId { get; set; } = Guid.NewGuid();
    public Guid ClaimId { get; set; }
    public Guid OrganisationId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public Guid? RelatedEntityId { get; set; }
    public string? RelatedEntityType { get; set; }
    public Guid? CorrelationId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public Guid? CreatedByUserId { get; set; }

    // Navigation
    public Claim? Claim { get; set; }
}

public class CauseOfLossCode
{
    public Guid CauseOfLossCodeId { get; set; } = Guid.NewGuid();
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string PerilCategory { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }
}

public class Policy
{
    public Guid PolicyId { get; set; } = Guid.NewGuid();
    public string PolicyNumber { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public DateOnly EffectiveDate { get; set; }
    public DateOnly ExpirationDate { get; set; }
    public PolicyStatus Status { get; set; }
    public string CoverageTypes { get; set; } = string.Empty;
}