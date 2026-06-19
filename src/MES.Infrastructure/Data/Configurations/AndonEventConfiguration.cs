using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MES.Domain.Entities;

namespace MES.Infrastructure.Data.Configurations;

public class AndonEventConfiguration : IEntityTypeConfiguration<AndonEvent>
{
    public void Configure(EntityTypeBuilder<AndonEvent> builder)
    {
        builder.ToTable("mes_andon_events");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.EventType).HasConversion<int>().IsRequired();
        builder.Property(e => e.Level).HasConversion<int>().IsRequired();
        builder.Property(e => e.Title).HasMaxLength(200).IsRequired();
        builder.Property(e => e.Description).HasMaxLength(1000);
        builder.Property(e => e.WorkstationName).HasMaxLength(100);
        builder.Property(e => e.WorkOrderNo).HasMaxLength(50);
        builder.Property(e => e.TriggeredByName).HasMaxLength(100);
        builder.Property(e => e.ResolvedByName).HasMaxLength(100);
        builder.Property(e => e.IsDeleted).HasDefaultValue(false);

        builder.HasIndex(e => e.WorkOrderId);
        builder.HasIndex(e => e.TriggeredAt);
        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}