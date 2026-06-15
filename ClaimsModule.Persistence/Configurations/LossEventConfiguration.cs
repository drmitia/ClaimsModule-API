using ClaimsModule.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClaimsModule.Persistence.Configurations;

public class LossEventConfiguration : IEntityTypeConfiguration<LossEvent>
{
    public void Configure(EntityTypeBuilder<LossEvent> builder)
    {
        builder.ToTable("LossEvents");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasDefaultValueSql("NEWSEQUENTIALID()");

        builder.Property(x => x.LossDescription)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(x => x.LossLocation)
            .HasMaxLength(500);

        builder.Property(x => x.CauseOfLossCode)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(x => x.EstimatedLossAmount)
            .HasColumnType("decimal(19,4)");

        builder.Property(x => x.LossDate)
            .HasColumnType("datetimeoffset(7)");

        builder.Property(x => x.ReportDate)
            .HasColumnType("datetimeoffset(7)");

        builder.Property(x => x.CreatedAt)
            .HasColumnType("datetimeoffset(7)");

        builder.Property(x => x.PoliceReportNumber)
            .HasMaxLength(100);

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}