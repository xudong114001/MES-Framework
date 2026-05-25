using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MES.Domain.Entities;

namespace MES.Infrastructure.Data.Configurations;

public class RoutingConfiguration : IEntityTypeConfiguration<Routing>
{
    public void Configure(EntityTypeBuilder<Routing> builder)
    {
        builder.ToTable("mes_routing");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.RoutingCode).HasMaxLength(50).IsRequired();
        builder.Property(e => e.RoutingName).HasMaxLength(200).IsRequired();
        builder.Property(e => e.Version).HasMaxLength(20).HasDefaultValue("V1.0");
        builder.Property(e => e.Status).HasDefaultValue(true);
        builder.Property(e => e.IsDeleted).HasDefaultValue(false);
        builder.HasQueryFilter(e => !e.IsDeleted);

        builder.HasOne(e => e.Material)
            .WithMany()
            .HasForeignKey(e => e.MaterialId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.Steps)
            .WithOne(s => s.Routing)
            .HasForeignKey(s => s.RoutingId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
