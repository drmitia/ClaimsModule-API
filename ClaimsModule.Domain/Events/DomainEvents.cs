using ClaimsModule.Domain.Enumerations;

namespace ClaimsModule.Domain.Events;

public abstract class DomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

public class ClaimCreatedEvent : DomainEvent
{
    public Guid ClaimId { get; }
    public string ClaimNumber { get; }
    public Guid OrganisationId { get; }
    public Guid? CreatedByUserId { get; }

    public ClaimCreatedEvent(Guid claimId, string claimNumber, 
        Guid organisationId, Guid? createdByUserId)
    {
        ClaimId = claimId;
        ClaimNumber = claimNumber;
        OrganisationId = organisationId;
        CreatedByUserId = createdByUserId;
    }
}

public class ClaimStatusChangedEvent : DomainEvent
{
    public Guid ClaimId { get; }
    public ClaimStatus FromStatus { get; }
    public ClaimStatus ToStatus { get; }
    public Guid? ChangedByUserId { get; }

    public ClaimStatusChangedEvent(Guid claimId, ClaimStatus from, 
        ClaimStatus to, Guid? changedByUserId)
    {
        ClaimId = claimId;
        FromStatus = from;
        ToStatus = to;
        ChangedByUserId = changedByUserId;
    }
}

public class ReserveApprovedEvent : DomainEvent
{
    public Guid ClaimId { get; }
    public Guid ReserveHistoryId { get; }
    public string IdempotencyKey { get; }

    public ReserveApprovedEvent(Guid claimId, Guid reserveHistoryId, 
        string idempotencyKey)
    {
        ClaimId = claimId;
        ReserveHistoryId = reserveHistoryId;
        IdempotencyKey = idempotencyKey;
    }
}