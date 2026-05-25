using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MES.Domain.Entities;

namespace MES.Infrastructure.Data.Configurations;

public class WorkshopConfiguration : IEntityTypeConfiguration<Workshop>
{
    public void Configure(EntityTypeBuilder<Workshop> builder)
    {
        builder.ToTable("mes_workshop");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Code).HasMaxLength(50).IsRequired();
        builder.Property(e => e.Name).HasMaxLength(200).IsRequired();
        builder.Property(e => e.Status).HasDefaultValue(true);
        builder.Property(e => e.IsDeleted).HasDefaultValue(false);
        builder.HasIndex(e => new { e.FactoryId, e.Code }).IsUnique();
        builder.HasQueryFilter(e => !e.IsDeleted);

        builder.HasOne(e => e.Factory)
            .WithMany(f => f.Workshops)
            .HasForeignKey(e => e.FactoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
