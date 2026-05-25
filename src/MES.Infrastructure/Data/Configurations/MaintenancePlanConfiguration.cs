using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MES.Domain.Entities;

namespace MES.Infrastructure.Data.Configurations;

public class MaintenancePlanConfiguration : IEntityTypeConfiguration<MaintenancePlan>
{
    public void Configure(EntityTypeBuilder<MaintenancePlan> builder)
    {
        builder.ToTable("mes_maintenance_plan");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.PlanName).HasMaxLength(200).IsRequired();
        builder.Property(e => e.Description).HasMaxLength(500);
        builder.Property(e => e.Status).HasConversion<int>().HasDefaultValue(Domain.Enums.MaintenancePlanStatus.PENDING);
        builder.Property(e => e.IsDeleted).HasDefaultValue(false);

        builder.HasOne(e => e.Equipment)
               .WithMany()
               .HasForeignKey(e => e.EquipmentId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
