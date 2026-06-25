using MES.Domain.AggregateRoots;
using MES.Domain.Exceptions;

namespace MES.Domain.Entities;

public class Workshop : BaseEntity, IAggregateRoot
{
    internal Workshop() { }

    public static Workshop Create(string code, string name, long factoryId)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new DomainException("车间编码不能为空", "WORKSHOP_CODE_REQUIRED");
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("车间名称不能为空", "WORKSHOP_NAME_REQUIRED");
        if (factoryId <= 0)
            throw new DomainException("所属工厂ID无效", "WORKSHOP_FACTORY_ID_INVALID");

        return new Workshop
        {
            Code = code,
            Name = name,
            FactoryId = factoryId,
            Status = true
        };
    }

    public long FactoryId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool Status { get; set; } = true;

    public virtual Factory? Factory { get; set; }
    public virtual ICollection<ProductionLine> ProductionLines { get; set; } = new List<ProductionLine>();

    /// <summary>
    /// 停用车间
    /// </summary>
    public void Deactivate()
    {
        if (!Status)
            throw new DomainException("车间已经处于停用状态", "WORKSHOP_ALREADY_INACTIVE");

        Status = false;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// 启用车间
    /// </summary>
    public void Activate()
    {
        if (Status)
            throw new DomainException("车间已经处于启用状态", "WORKSHOP_ALREADY_ACTIVE");

        Status = true;
        UpdatedAt = DateTime.UtcNow;
    }
}
