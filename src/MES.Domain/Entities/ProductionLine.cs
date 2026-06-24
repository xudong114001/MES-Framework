using MES.Domain.AggregateRoots;
using MES.Domain.Enums;
using MES.Domain.Exceptions;

namespace MES.Domain.Entities;

public class ProductionLine : BaseEntity, IAggregateRoot
{
    internal ProductionLine() { }

    public static ProductionLine Create(string code, string name, long workshopId, LineType lineType)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new DomainException("产线编码不能为空", "LINE_CODE_REQUIRED");
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("产线名称不能为空", "LINE_NAME_REQUIRED");
        if (workshopId <= 0)
            throw new DomainException("所属车间ID无效", "LINE_WORKSHOP_ID_INVALID");

        return new ProductionLine
        {
            Code = code,
            Name = name,
            WorkshopId = workshopId,
            LineType = lineType,
            Status = true
        };
    }

    public long WorkshopId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public LineType LineType { get; set; }
    public bool Status { get; set; } = true;

    public virtual Workshop? Workshop { get; set; }
    public virtual ICollection<Workstation> Workstations { get; set; } = new List<Workstation>();

    /// <summary>
    /// 停用产线
    /// </summary>
    public void Deactivate()
    {
        if (!Status)
            throw new DomainException("产线已经处于停用状态", "LINE_ALREADY_INACTIVE");

        Status = false;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// 启用产线
    /// </summary>
    public void Activate()
    {
        if (Status)
            throw new DomainException("产线已经处于启用状态", "LINE_ALREADY_ACTIVE");

        Status = true;
        UpdatedAt = DateTime.UtcNow;
    }
}
