using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MES.Domain.Entities;

namespace MES.Infrastructure.Data.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("mes_role");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Name).HasMaxLength(50).IsRequired();
        builder.Property(e => e.Description).HasMaxLength(200);
        builder.Property(e => e.IsDeleted).HasDefaultValue(false);
        builder.HasIndex(e => e.Name).IsUnique();
        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}
