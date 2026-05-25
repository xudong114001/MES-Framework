using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MES.Domain.Entities;

namespace MES.Infrastructure.Data.Configurations;

public class WorkOrderStepConfiguration : IEntityTypeConfiguration<WorkOrderStep>
{
    public void Configure(EntityTypeBuilder<WorkOrderStep> builder)
    {
        builder.ToTable("mes_work_order_step");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.StepName).HasMaxLength(200).IsRequired();
        builder.Property(e => e.PlannedQty).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(e => e.CompletedQty).HasColumnType("decimal(18,2)").HasDefaultValue(0);
        builder.Property(e => e.ScrapQty).HasColumnType("decimal(18,2)").HasDefaultValue(0);
        builder.Property(e => e.Status).HasConversion<int>().HasDefaultValue(MES.Domain.Enums.WorkOrderStatus.PENDING);
        builder.HasIndex(e => new { e.WorkOrderId, e.StepNo }).IsUnique();

        builder.HasOne(e => e.WorkOrder)
            .WithMany(wo => wo.Steps)
            .HasForeignKey(e => e.WorkOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Workstation)
            .WithMany()
            .HasForeignKey(e => e.WorkstationId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
