using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MES.Domain.Entities;

namespace MES.Infrastructure.Data.Configurations;

public class WorkstationConfiguration : IEntityTypeConfiguration<Workstation>
{
    public void Configure(EntityTypeBuilder<Workstation> builder)
    {
        builder.ToTable("mes_workstation");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Code).HasMaxLength(50).IsRequired();
        builder.Property(e => e.Name).HasMaxLength(200).IsRequired();
        builder.Property(e => e.SeqNo).HasDefaultValue(0);
        builder.Property(e => e.Status).HasDefaultValue(true);
        builder.Property(e => e.IsDeleted).HasDefaultValue(false);
        builder.HasIndex(e => new { e.LineId, e.Code }).IsUnique();
        builder.HasQueryFilter(e => !e.IsDeleted);

        builder.HasOne(e => e.ProductionLine)
            .WithMany(pl => pl.Workstations)
            .HasForeignKey(e => e.LineId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
