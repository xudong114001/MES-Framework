using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MES.Domain.Entities;

namespace MES.Infrastructure.Data.Configurations;

public class AlertRuleConfiguration : IEntityTypeConfiguration<AlertRule>
{
    public void Configure(EntityTypeBuilder<AlertRule> builder)
    {
        builder.HasQueryFilter(e => !e.IsDeleted);
        builder.ToTable("mes_alert_rule");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Name).HasMaxLength(200).IsRequired();
        builder.Property(e => e.Description).HasMaxLength(500);
        builder.Property(e => e.Condition).HasMaxLength(500).IsRequired();
        builder.Property(e => e.Level).HasConversion<int>().IsRequired();
    }
}
