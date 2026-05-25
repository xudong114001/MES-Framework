using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MES.Domain.Entities;

namespace MES.Infrastructure.Data.Configurations;

public class MaterialTraceConfiguration : IEntityTypeConfiguration<MaterialTrace>
{
    public void Configure(EntityTypeBuilder<MaterialTrace> builder)
    {
        builder.ToTable("mes_material_trace");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.BatchNo).HasMaxLength(100);
        builder.Property(e => e.SerialNo).HasMaxLength(100);
        builder.Property(e => e.Direction).HasMaxLength(20);
        builder.Property(e => e.Qty).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(e => e.Remark).HasMaxLength(500);
        builder.HasIndex(e => e.BatchNo);
        builder.HasIndex(e => e.SerialNo);

        builder.HasOne(e => e.Material)
            .WithMany()
            .HasForeignKey(e => e.MaterialId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
