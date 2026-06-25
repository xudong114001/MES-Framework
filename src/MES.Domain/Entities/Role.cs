using MES.Domain.AggregateRoots;
using MES.Domain.Exceptions;

namespace MES.Domain.Entities;

public class Role : BaseEntity, IAggregateRoot
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    internal Role() { }

    public static Role Create(string name, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("角色名称不能为空");

        return new Role
        {
            Name = name,
            Description = description
        };
    }

    public void UpdateInfo(string name, string? description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("角色名称不能为空");

        Name = name;
        Description = description;
    }
}
