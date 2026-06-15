using ClaimsModule.Domain.Entities;
using ClaimsModule.Domain.Enumerations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClaimsModule.Persistence.Configurations;

public class ClaimConfiguration : IEntityTypeConfiguration<Claim>
{
    public void Configure(EntityTypeBuilder<Claim> builder)
    {
        builder.ToTable("Claims");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasDefaultValueSql("NEWSEQUENTIALID()");

        builder.Property(x => x.ClaimNumber)
            .IsRequired()
            .HasMaxLength(20);

        builder.HasIndex(x => x.ClaimNumber)
            .IsUnique();

        builder.Property(x => x.PolicyNumber)
            .HasMaxLength(50);

        builder.Property(x => x.ClientName)
            .HasMaxLength(200);

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(x => x.Severity)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(x => x.ReportedDate)
            .HasColumnType("datetimeoffset(7)");

        builder.Property(x => x.CreatedAt)
            .HasColumnType("datetimeoffset(7)");

        builder.Property(x => x.UpdatedAt)
            .HasColumnType("datetimeoffset(7)");

        builder.Property(x => x.ClosedAt)
            .HasColumnType("datetimeoffset(7)");

        builder.Property(x => x.DeletedAt)
            .HasColumnType("datetimeoffset(7)");

        builder.Property(x => x.Notes)
            .HasMaxLength(4000);

        builder.Property(x => x.ClosureReason)
            .HasMaxLength(1000);

        // Optimistic concurrency
        builder.Property(x => x.RowVer)
            .IsRowVersion();

        // Soft delete global query filter
        builder.HasQueryFilter(x => !x.IsDeleted);

        // Relationships
        builder.HasOne(x => x.LossEvent)
            .WithOne(x => x.Claim)
            .HasForeignKey<LossEvent>(x => x.ClaimId);

        builder.HasMany(x => x.Parties)
            .WithOne(x => x.Claim)
            .HasForeignKey(x => x.ClaimId);

        builder.HasMany(x => x.RiskObjects)
            .WithOne(x => x.Claim)
            .HasForeignKey(x => x.ClaimId);

        builder.HasMany(x => x.ReserveComponents)
            .WithOne(x => x.Claim)
            .HasForeignKey(x => x.ClaimId);

        builder.HasMany(x => x.Documents)
            .WithOne(x => x.Claim)
            .HasForeignKey(x => x.ClaimId);

        builder.HasMany(x => x.AuditLogs)
            .WithOne(x => x.Claim)
            .HasForeignKey(x => x.ClaimId);

        // Indexes
        builder.HasIndex(x => x.OrganisationId);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.AssignedHandlerId);
    }
}