using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MES.Domain.Entities;

namespace MES.Infrastructure.Data.Configurations;

public class QcInspectionConfiguration : IEntityTypeConfiguration<QcInspection>
{
    public void Configure(EntityTypeBuilder<QcInspection> builder)
    {
        builder.ToTable("mes_qc_inspection");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.InspectNo).HasMaxLength(50).IsRequired();
        builder.Property(e => e.SourceType).HasConversion<int>().HasDefaultValue(MES.Domain.Enums.QcInspectionType.INCOMING);
        builder.Property(e => e.SourceRef).HasMaxLength(100);
        builder.Property(e => e.InspectResult).HasConversion<int>().HasDefaultValue(MES.Domain.Enums.QcResult.PENDING);
        builder.Property(e => e.Remark).HasMaxLength(500);
        builder.Property(e => e.HandlingAction).HasMaxLength(50);
        builder.Property(e => e.HandlingRemark).HasMaxLength(500);
        builder.HasIndex(e => e.InspectNo).IsUnique();

        builder.HasMany(e => e.Items)
            .WithOne(i => i.QcInspection)
            .HasForeignKey(i => i.InspectionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
