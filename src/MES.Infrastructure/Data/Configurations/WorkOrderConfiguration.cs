using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MES.Domain.Entities;
using MES.Domain.ValueObjects;
using MES.Infrastructure.Data.Converters;

namespace MES.Infrastructure.Data.Configurations;

public class WorkOrderConfiguration : IEntityTypeConfiguration<WorkOrder>
{
    public void Configure(EntityTypeBuilder<WorkOrder> builder)
    {
        builder.ToTable("mes_work_order");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.OrderNo).HasMaxLength(50).IsRequired();
        builder.Property(e => e.SourceRef).HasMaxLength(100);
        builder.Property(e => e.PlannedQty).HasColumnType("decimal(18,2)").HasConversion(new QuantityValueConverter()).IsRequired();
        builder.Property(e => e.CompletedQty).HasColumnType("decimal(18,2)").HasConversion(new QuantityValueConverter()).HasDefaultValue(new Quantity(0));
        builder.Property(e => e.ScrapQty).HasColumnType("decimal(18,2)").HasConversion(new QuantityValueConverter()).HasDefaultValue(new Quantity(0));
        builder.Property(e => e.Status).HasConversion<int>().HasDefaultValue(MES.Domain.Enums.WorkOrderStatus.PENDING);
        builder.Property(e => e.Priority).HasConversion<int>().HasDefaultValue(MES.Domain.Enums.Priority.NORMAL);
        builder.Property(e => e.Remark).HasMaxLength(500);
        builder.Property(e => e.IsDeleted).HasDefaultValue(false);
        builder.HasIndex(e => e.OrderNo).IsUnique();
        builder.HasIndex(e => e.Status);
        builder.HasQueryFilter(e => !e.IsDeleted);

        builder.HasOne(e => e.Material)
            .WithMany()
            .HasForeignKey(e => e.MaterialId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Routing)
            .WithMany()
            .HasForeignKey(e => e.RoutingId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(e => e.Steps)
            .WithOne(s => s.WorkOrder)
            .HasForeignKey(s => s.WorkOrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
