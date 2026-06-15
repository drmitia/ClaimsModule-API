namespace ClaimsModule.Domain.Enumerations;

public enum PartyRole
{
    Claimant,
    Insured,
    ThirdParty,
    Witness,
    Attorney
}

public enum PartyType
{
    Person,
    Company
}

public enum AssetType
{
    Vehicle,
    Property,
    Person,
    Equipment,
    Other
}

public enum ReserveComponent
{
    Indemnity,
    Expense,
    ALAE,
    SubrogationRecoverable
}

public enum ApprovalStatus
{
    AutoApproved,
    PendingApproval,
    Approved,
    Rejected,
    Cancelled
}

public enum PostingStatus
{
    Pending,
    Posted,
    Failed,
    Cancelled
}

public enum TransactionType
{
    Add,
    Adjust,
    Reverse
}

public enum ClaimSeverity
{
    Minor,
    Standard,
    Critical,
    Catastrophic
}

public enum DocumentType
{
    PoliceReport,
    MedicalReport,
    Invoice,
    Photo,
    Other
}

public enum UserRole
{
    Handler,
    Supervisor,
    Manager
}

public enum PolicyStatus
{
    Active,
    Expired,
    Cancelled
}