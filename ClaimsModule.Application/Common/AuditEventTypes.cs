namespace ClaimsModule.Application.Common;

public static class AuditEventTypes
{
    public const string ClaimCreated = "CLAIM_CREATED";
    public const string ClaimStatusChanged = "CLAIM_STATUS_CHANGED";
    public const string ClaimUpdated = "CLAIM_UPDATED";
    public const string ClaimClosed = "CLAIM_CLOSED";
    public const string ClaimReopened = "CLAIM_REOPENED";
    public const string ClaimWithdrawn = "CLAIM_WITHDRAWN";

    public const string PartyAdded = "PARTY_ADDED";
    public const string PartyUpdated = "PARTY_UPDATED";
    public const string PartyRemoved = "PARTY_REMOVED";

    public const string RiskObjectAdded = "RISK_OBJECT_ADDED";
    public const string RiskObjectUpdated = "RISK_OBJECT_UPDATED";
    public const string RiskObjectRemoved = "RISK_OBJECT_REMOVED";

    public const string ReserveCreated = "RESERVE_CREATED";
    public const string ReserveAutoApproved = "RESERVE_AUTO_APPROVED";
    public const string ReservePendingApproval = "RESERVE_PENDING_APPROVAL";
    public const string ReserveApproved = "RESERVE_APPROVED";
    public const string ReserveRejected = "RESERVE_REJECTED";
    public const string ReserveRetracted = "RESERVE_RETRACTED";

    public const string DocumentUploaded = "DOCUMENT_UPLOADED";
    public const string DocumentDeleted = "DOCUMENT_DELETED";

    public const string GlPostingSimulated = "GL_POSTING_SIMULATED";
    public const string SlaBreachFlagged = "SLA_BREACH_FLAGGED";
    
    public const string PolicyUnknown = "POLICY_UNKNOWN";
}