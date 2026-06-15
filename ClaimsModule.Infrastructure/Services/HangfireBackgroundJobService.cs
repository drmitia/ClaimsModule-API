using ClaimsModule.Application.Common.Interfaces;
using ClaimsModule.Infrastructure.Jobs;
using Hangfire;

namespace ClaimsModule.Infrastructure.Services;

public class HangfireBackgroundJobService : IBackgroundJobService
{
    private readonly IBackgroundJobClient _backgroundJobClient;

    public HangfireBackgroundJobService(IBackgroundJobClient backgroundJobClient)
    {
        _backgroundJobClient = backgroundJobClient;
    }

    public void EnqueueGLPosting(Guid reserveHistoryId, string idempotencyKey)
    {
        _backgroundJobClient.Enqueue<PostGLReserveChangeJob>(
            job => job.ExecuteAsync(reserveHistoryId, idempotencyKey, CancellationToken.None));
    }
}