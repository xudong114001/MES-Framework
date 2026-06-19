namespace MES.Domain.Entities;

/// <summary>
/// 角色权限关联表 — RBAC 权限分配
/// </summary>
public class RolePermission : BaseEntity
{
    public long RoleId { get; set; }
    public string Permission { get; set; } = string.Empty;

    public Role? Role { get; set; }
}
