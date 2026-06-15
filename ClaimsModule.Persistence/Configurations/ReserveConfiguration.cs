using ClaimsModule.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClaimsModule.Persistence.Configurations;

public class ReserveConfiguration : IEntityTypeConfiguration<ClaimReserveComponent>
{
    public void Configure(EntityTypeBuilder<ClaimReserveComponent> builder)
    {
        builder.ToTable("ClaimReserveComponents");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasDefaultValueSql("NEWSEQUENTIALID()");

        builder.Property(x => x.Component)
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(x => x.CurrentAmount)
            .HasColumnType("decimal(19,4)");

        builder.Property(x => x.Notes)
            .HasMaxLength(1000);

        builder.Property(x => x.CreatedAt)
            .HasColumnType("datetimeoffset(7)");

        builder.Property(x => x.RowVer)
            .IsRowVersion();

        builder.HasQueryFilter(x => !x.IsDeleted);

        builder.HasMany(x => x.History)
            .WithOne(x => x.ReserveComponent)
            .HasForeignKey(x => x.ReserveComponentId);

        builder.HasIndex(x => x.ClaimId);
    }
}

public class ReserveHistoryConfiguration : IEntityTypeConfiguration<ReserveHistory>
{
    public void Configure(EntityTypeBuilder<ReserveHistory> builder)
    {
        builder.ToTable("ReserveHistory");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasDefaultValueSql("NEWSEQUENTIALID()");

        builder.Property(x => x.TransactionType)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(x => x.ApprovalStatus)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(x => x.PostingStatus)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(x => x.Amount)
            .HasColumnType("decimal(19,4)");

        builder.Property(x => x.PreviousBalance)
            .HasColumnType("decimal(19,4)");

        builder.Property(x => x.NewBalance)
            .HasColumnType("decimal(19,4)");

        builder.Property(x => x.ChangeReason)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(x => x.RejectionReason)
            .HasMaxLength(1000);

        builder.Property(x => x.IdempotencyKey)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(x => x.IdempotencyKey)
            .IsUnique();

        builder.Property(x => x.CreatedAt)
            .HasColumnType("datetimeoffset(7)");

        builder.Property(x => x.ApprovedAt)
            .HasColumnType("datetimeoffset(7)");

        builder.Property(x => x.RejectedAt)
            .HasColumnType("datetimeoffset(7)");

        // Append-only — no soft delete, no update filter
        builder.HasIndex(x => x.ClaimId);
        builder.HasIndex(x => x.ReserveComponentId);
    }
}