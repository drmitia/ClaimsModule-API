namespace ClaimsModule.Domain.Exceptions;

public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}

public class InvalidStatusTransitionException : DomainException
{
    public InvalidStatusTransitionException(string from, string to)
        : base($"Transition from '{from}' to '{to}' is not permitted.") { }
}

public class ClaimNotFoundException : DomainException
{
    public ClaimNotFoundException(Guid claimId)
        : base($"Claim with ID '{claimId}' was not found.") { }
}

public class ReserveNotFoundException : DomainException
{
    public ReserveNotFoundException(Guid reserveId)
        : base($"Reserve transaction with ID '{reserveId}' was not found.") { }
}

public class SelfApprovalException : DomainException
{
    public SelfApprovalException()
        : base("Self-approval is not permitted.") { }
}

public class InsufficientAuthorityException : DomainException
{
    public InsufficientAuthorityException(string message)
        : base(message) { }
}