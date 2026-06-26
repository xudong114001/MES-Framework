using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MES.Domain.Entities;

namespace MES.Infrastructure.Data.Configurations;

public class RoutingStepConfiguration : IEntityTypeConfiguration<RoutingStep>
{
    public void Configure(EntityTypeBuilder<RoutingStep> builder)
    {
        builder.HasQueryFilter(e => !e.IsDeleted);
        builder.ToTable("mes_routing_step");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.StepName).HasMaxLength(200).IsRequired();
        builder.Property(e => e.WorkstationType).HasMaxLength(100);
        builder.Property(e => e.StandardTime).HasColumnType("decimal(10,2)").HasDefaultValue(0);
        builder.Property(e => e.IsQcPoint).HasDefaultValue(false);
        builder.Property(e => e.IsScrapPoint).HasDefaultValue(false);
        builder.HasIndex(e => new { e.RoutingId, e.StepNo }).IsUnique();

        builder.HasOne(e => e.Routing)
            .WithMany(r => r.Steps)
            .HasForeignKey(e => e.RoutingId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
