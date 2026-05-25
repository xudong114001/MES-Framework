using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MES.Domain.Entities;

namespace MES.Infrastructure.Data.Configurations;

public class WorkReportConfiguration : IEntityTypeConfiguration<WorkReport>
{
    public void Configure(EntityTypeBuilder<WorkReport> builder)
    {
        builder.ToTable("mes_work_report");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.ReportNo).HasMaxLength(50).IsRequired();
        builder.Property(e => e.ReportType).HasConversion<int>().HasDefaultValue(MES.Domain.Enums.ReportType.COMPLETE);
        builder.Property(e => e.GoodQty).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(e => e.ScrapQty).HasColumnType("decimal(18,2)").HasDefaultValue(0);
        builder.Property(e => e.ReworkQty).HasColumnType("decimal(18,2)").HasDefaultValue(0);
        builder.Property(e => e.Remark).HasMaxLength(500);
        builder.HasIndex(e => e.ReportNo).IsUnique();

        builder.HasOne(e => e.WorkOrder)
            .WithMany()
            .HasForeignKey(e => e.WorkOrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.WorkOrderStep)
            .WithMany()
            .HasForeignKey(e => e.StepId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(e => e.Workstation)
            .WithMany()
            .HasForeignKey(e => e.WorkstationId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
