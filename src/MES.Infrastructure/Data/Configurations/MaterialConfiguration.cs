using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MES.Domain.Entities;

namespace MES.Infrastructure.Data.Configurations;

public class MaterialConfiguration : IEntityTypeConfiguration<Material>
{
    public void Configure(EntityTypeBuilder<Material> builder)
    {
        builder.ToTable("mes_material");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Code).HasMaxLength(50).IsRequired();
        builder.Property(e => e.Name).HasMaxLength(200).IsRequired();
        builder.Property(e => e.Spec).HasMaxLength(200);
        builder.Property(e => e.Unit).HasMaxLength(20);
        builder.Property(e => e.Category).HasMaxLength(50);
        builder.Property(e => e.StockQty).HasColumnType("decimal(18,4)").HasDefaultValue(0);
        builder.Property(e => e.Status).HasDefaultValue(true);
        builder.Property(e => e.IsDeleted).HasDefaultValue(false);
        builder.HasIndex(e => e.Code).IsUnique();
        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}
