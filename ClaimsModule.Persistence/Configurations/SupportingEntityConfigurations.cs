using ClaimsModule.Domain.Entities;
using ClaimsModule.Domain.Enumerations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ClaimsModule.Persistence.Seeders;

namespace ClaimsModule.Persistence.Configurations;

public class ClaimDocumentConfiguration : IEntityTypeConfiguration<ClaimDocument>
{
    public void Configure(EntityTypeBuilder<ClaimDocument> builder)
    {
        builder.ToTable("ClaimDocuments");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasDefaultValueSql("NEWSEQUENTIALID()");

        builder.Property(x => x.DocumentType)
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(x => x.DocumentName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.BlobPath)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(x => x.ContentType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Notes)
            .HasMaxLength(1000);

        builder.Property(x => x.UploadedAt)
            .HasColumnType("datetimeoffset(7)");

        builder.Property(x => x.CreatedAt)
            .HasColumnType("datetimeoffset(7)");

        builder.HasQueryFilter(x => !x.IsDeleted);

        builder.HasIndex(x => x.ClaimId);
    }
}

public class ClaimAuditLogConfiguration : IEntityTypeConfiguration<ClaimAuditLog>
{
    public void Configure(EntityTypeBuilder<ClaimAuditLog> builder)
    {
        builder.ToTable("ClaimAuditLog");

        builder.HasKey(x => x.AuditLogId);

        builder.Property(x => x.AuditLogId)
            .HasDefaultValueSql("NEWSEQUENTIALID()");

        builder.Property(x => x.EventType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Description)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(x => x.OldValue)
            .HasMaxLength(4000);

        builder.Property(x => x.NewValue)
            .HasMaxLength(4000);

        builder.Property(x => x.RelatedEntityType)
            .HasMaxLength(50);

        builder.Property(x => x.CreatedAt)
            .HasColumnType("datetimeoffset(7)");

        // Immutable — no update, no delete, no soft delete filter
        builder.HasIndex(x => x.ClaimId);
        builder.HasIndex(x => x.OrganisationId);
        builder.HasIndex(x => x.CreatedAt);
    }
}

public class CauseOfLossCodeConfiguration : IEntityTypeConfiguration<CauseOfLossCode>
{
    public void Configure(EntityTypeBuilder<CauseOfLossCode> builder)
    {
        builder.ToTable("CauseOfLossCodes");

        builder.HasKey(x => x.CauseOfLossCodeId);

        builder.Property(x => x.Code)
            .IsRequired()
            .HasMaxLength(20);

        builder.HasIndex(x => x.Code)
            .IsUnique();

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.PerilCategory)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasData(SeedData.CauseOfLossCodes);
    }
}

public class PolicyConfiguration : IEntityTypeConfiguration<Policy>
{
    public void Configure(EntityTypeBuilder<Policy> builder)
    {
        builder.ToTable("Policies");

        builder.HasKey(x => x.PolicyId);

        builder.Property(x => x.PolicyNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(x => x.PolicyNumber)
            .IsUnique();

        builder.Property(x => x.ClientName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(x => x.CoverageTypes)
            .HasMaxLength(500);

        builder.HasData(SeedData.Policies);
    }
}