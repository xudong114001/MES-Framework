using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MES.AI.Domain.Entities;

namespace MES.Infrastructure.Data.Configurations;

public class AlertRecordConfiguration : IEntityTypeConfiguration<AlertRecord>
{
    public void Configure(EntityTypeBuilder<AlertRecord> builder)
    {
        builder.ToTable("mes_alert_record");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.RuleName).HasMaxLength(200).IsRequired();
        builder.Property(e => e.Title).HasMaxLength(500).IsRequired();
        builder.Property(e => e.Message).HasMaxLength(2000).IsRequired();
        builder.Property(e => e.Level).HasConversion<int>().IsRequired();
        builder.Property(e => e.RelatedEntityType).HasMaxLength(100);
    }
}
