using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MES.Domain.Entities;

namespace MES.Infrastructure.Data.Configurations;

public class BomConfiguration : IEntityTypeConfiguration<Bom>
{
    public void Configure(EntityTypeBuilder<Bom> builder)
    {
        builder.ToTable("mes_bom");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Quantity).HasColumnType("decimal(18,6)").IsRequired();
        builder.Property(e => e.ScrapRate).HasColumnType("decimal(5,2)").HasDefaultValue(0);
        builder.Property(e => e.SeqNo).HasDefaultValue(0);
        builder.Property(e => e.Status).HasDefaultValue(true);
        builder.Property(e => e.IsDeleted).HasDefaultValue(false);
        builder.HasIndex(e => e.ProductId);
        builder.HasQueryFilter(e => !e.IsDeleted);

        builder.HasOne(e => e.Product)
            .WithMany()
            .HasForeignKey(e => e.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Material)
            .WithMany()
            .HasForeignKey(e => e.MaterialId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
