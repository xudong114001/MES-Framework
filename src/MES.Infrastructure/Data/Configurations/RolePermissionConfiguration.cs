using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MES.Domain.Entities;

namespace MES.Infrastructure.Data.Configurations;

public class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> builder)
    {
        builder.HasQueryFilter(e => !e.IsDeleted);
        builder.ToTable("mes_role_permission");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Permission).HasMaxLength(200).IsRequired();
        builder.HasIndex(e => new { e.RoleId, e.Permission }).IsUnique();
    }
}
