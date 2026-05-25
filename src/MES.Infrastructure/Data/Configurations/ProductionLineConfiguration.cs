using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MES.Domain.Entities;

namespace MES.Infrastructure.Data.Configurations;

public class ProductionLineConfiguration : IEntityTypeConfiguration<ProductionLine>
{
    public void Configure(EntityTypeBuilder<ProductionLine> builder)
    {
        builder.ToTable("mes_production_line");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Code).HasMaxLength(50).IsRequired();
        builder.Property(e => e.Name).HasMaxLength(200).IsRequired();
        builder.Property(e => e.LineType).HasConversion<int>().HasDefaultValue(MES.Domain.Enums.LineType.FLOW);
        builder.Property(e => e.Status).HasDefaultValue(true);
        builder.Property(e => e.IsDeleted).HasDefaultValue(false);
        builder.HasIndex(e => new { e.WorkshopId, e.Code }).IsUnique();
        builder.HasQueryFilter(e => !e.IsDeleted);

        builder.HasOne(e => e.Workshop)
            .WithMany(w => w.ProductionLines)
            .HasForeignKey(e => e.WorkshopId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
