namespace ClaimsModule.Application.Common.Interfaces;

public interface IBackgroundJobService
{
    void EnqueueGLPosting(Guid reserveHistoryId, string idempotencyKey);
}