using ClaimsModule.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClaimsModule.Persistence.Configurations;

public class ClaimRiskObjectConfiguration : IEntityTypeConfiguration<ClaimRiskObject>
{
    public void Configure(EntityTypeBuilder<ClaimRiskObject> builder)
    {
        builder.ToTable("ClaimRiskObjects");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasDefaultValueSql("NEWSEQUENTIALID()");

        builder.Property(x => x.AssetType)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(x => x.AssetDescription)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.DamageDescription)
            .HasMaxLength(1000);

        builder.Property(x => x.AssetReference)
            .HasMaxLength(100);

        builder.Property(x => x.CreatedAt)
            .HasColumnType("datetimeoffset(7)");

        builder.HasQueryFilter(x => !x.IsDeleted);

        builder.HasIndex(x => x.ClaimId);
    }
}