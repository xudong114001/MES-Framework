using MES.Domain.AggregateRoots;
using MES.Domain.Exceptions;

namespace MES.Domain.Entities;

public class Factory : BaseEntity, IAggregateRoot
{
    internal Factory() { }

    public static Factory Create(string code, string name, string? address = null)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new DomainException("工厂编码不能为空", "FACTORY_CODE_REQUIRED");
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("工厂名称不能为空", "FACTORY_NAME_REQUIRED");

        return new Factory
        {
            Code = code,
            Name = name,
            Address = address,
            Status = true
        };
    }

    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public bool Status { get; set; } = true;

    public virtual ICollection<Workshop> Workshops { get; set; } = new List<Workshop>();

    /// <summary>
    /// 停用工厂
    /// </summary>
    public void Deactivate()
    {
        if (!Status)
            throw new DomainException("工厂已经处于停用状态", "FACTORY_ALREADY_INACTIVE");

        Status = false;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// 启用工厂
    /// </summary>
    public void Activate()
    {
        if (Status)
            throw new DomainException("工厂已经处于启用状态", "FACTORY_ALREADY_ACTIVE");

        Status = true;
        UpdatedAt = DateTime.UtcNow;
    }
}
