using MES.Domain.Enums;
using MES.Domain.Exceptions;

namespace MES.Domain.Entities;

public class MaintenancePlan : BaseEntity
{
    public long EquipmentId { get; private set; }
    public string PlanName { get; private set; } = string.Empty;
    public int CycleDays { get; private set; }
    public DateTime? LastCompletedDate { get; private set; }
    public DateTime NextDueDate { get; private set; }
    public string? Description { get; private set; }
    public MaintenancePlanStatus Status { get; private set; } = MaintenancePlanStatus.PENDING;

    public virtual Equipment? Equipment { get; set; }

    /// <summary>
    /// EF Core 需要的无参构造函数
    /// </summary>
    protected MaintenancePlan() { }

    /// <summary>
    /// 创建保养计划
    /// </summary>
    public MaintenancePlan(long equipmentId, string planName, int cycleDays, string? description = null)
    {
        if (equipmentId <= 0)
            throw new DomainException("设备ID无效");

        if (string.IsNullOrWhiteSpace(planName))
            throw new DomainException("保养计划名称不能为空");

        if (cycleDays <= 0)
            throw new DomainException("保养周期必须大于0");

        EquipmentId = equipmentId;
        PlanName = planName;
        CycleDays = cycleDays;
        Description = description;
        NextDueDate = DateTime.UtcNow.AddDays(cycleDays);
        Status = MaintenancePlanStatus.PENDING;
    }

    /// <summary>
    /// 完成保养
    /// </summary>
    public void Complete()
    {
        LastCompletedDate = DateTime.UtcNow;
        NextDueDate = DateTime.UtcNow.AddDays(CycleDays);
        Status = MaintenancePlanStatus.COMPLETED;
    }

    /// <summary>
    /// 更新保养周期
    /// </summary>
    public void UpdateCycleDays(int cycleDays)
    {
        if (cycleDays <= 0)
            throw new DomainException("保养周期必须大于0");

        CycleDays = cycleDays;
    }

    /// <summary>
    /// 更新计划名称
    /// </summary>
    public void UpdatePlanName(string planName)
    {
        if (string.IsNullOrWhiteSpace(planName))
            throw new DomainException("保养计划名称不能为空");

        PlanName = planName;
    }

    /// <summary>
    /// 更新描述
    /// </summary>
    public void UpdateDescription(string description)
    {
        Description = description;
    }

    /// <summary>
    /// 检查是否逾期
    /// </summary>
    public bool IsOverdue()
    {
        return Status != MaintenancePlanStatus.COMPLETED && DateTime.UtcNow > NextDueDate;
    }
}
