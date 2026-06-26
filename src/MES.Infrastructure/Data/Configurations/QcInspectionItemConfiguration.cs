using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MES.Domain.Entities;

namespace MES.Infrastructure.Data.Configurations;

public class QcInspectionItemConfiguration : IEntityTypeConfiguration<QcInspectionItem>
{
    public void Configure(EntityTypeBuilder<QcInspectionItem> builder)
    {
        builder.HasQueryFilter(e => !e.IsDeleted);
        builder.ToTable("mes_qc_inspection_item");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.ItemName).HasMaxLength(200).IsRequired();
        builder.Property(e => e.SpecValue).HasMaxLength(200);
        builder.Property(e => e.ActualValue).HasMaxLength(200);
        builder.Property(e => e.Result).HasConversion<int>().HasDefaultValue(MES.Domain.Enums.QcResult.PENDING);

        builder.HasOne(e => e.QcInspection)
            .WithMany(q => q.Items)
            .HasForeignKey(e => e.InspectionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
