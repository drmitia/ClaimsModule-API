namespace ClaimsModule.Application.Common.Interfaces;

public interface ICorrelationIdService
{
    Guid CorrelationId { get; }
}