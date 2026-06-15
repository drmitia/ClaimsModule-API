using ClaimsModule.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClaimsModule.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Claim> Claims { get; }
    DbSet<LossEvent> LossEvents { get; }
    DbSet<ClaimParty> ClaimParties { get; }
    DbSet<ClaimRiskObject> ClaimRiskObjects { get; }
    DbSet<ClaimReserveComponent> ClaimReserveComponents { get; }
    DbSet<ReserveHistory> ReserveHistories { get; }
    DbSet<ClaimDocument> ClaimDocuments { get; }
    DbSet<ClaimAuditLog> ClaimAuditLogs { get; }
    DbSet<CauseOfLossCode> CauseOfLossCodes { get; }
    DbSet<Policy> Policies { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}