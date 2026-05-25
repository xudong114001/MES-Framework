using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MES.Domain.Entities;

namespace MES.Infrastructure.Data.Configurations;

public class QcCheckpointConfiguration : IEntityTypeConfiguration<QcCheckpoint>
{
    public void Configure(EntityTypeBuilder<QcCheckpoint> builder)
    {
        builder.ToTable("mes_qc_checkpoint");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.CheckType).HasConversion<int>().IsRequired();
        builder.Property(e => e.Remark).HasMaxLength(500);
        builder.HasIndex(e => new { e.StepId, e.CheckType }).IsUnique();
    }
}
