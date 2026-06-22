using MES.Domain.AggregateRoots;
using MES.Domain.Enums;
using MES.Domain.Exceptions;

namespace MES.Domain.Entities;

public class Equipment : BaseEntity, IAggregateRoot
{
    private readonly List<MaintenancePlan> _maintenancePlans = new();

    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? Model { get; private set; }
    public long? FactoryId { get; private set; }
    public long? WorkshopId { get; private set; }
    public long? LineId { get; private set; }
    public DateTime? InstallDate { get; private set; }
    public EquipmentStatus Status { get; private set; }
    public DateTime? LastMaintainDate { get; private set; }
    public DateTime? NextMaintainDate { get; private set; }
    public int? MaintainCycle { get; private set; }

    /// <summary>
    /// 理论节拍（秒/件）
    /// </summary>
    public double? TheoreticalCycleTime { get; private set; }

    /// <summary>
    /// 日计划运行时间（小时）
    /// </summary>
    public double? PlannedRunTime { get; private set; }

    public IReadOnlyCollection<MaintenancePlan> MaintenancePlans => _maintenancePlans.AsReadOnly();

    /// <summary>
    /// EF Core 需要的无参构造函数（内部使用）
    /// </summary>
    internal Equipment() { }

    #region 工厂方法

    /// <summary>
    /// 创建设备
    /// </summary>
    public static Equipment Create(
        string code,
        string name,
        string? model = null,
        long? factoryId = null,
        long? workshopId = null,
        long? lineId = null,
        DateTime? installDate = null,
        int? maintainCycle = null,
        double? theoreticalCycleTime = null,
        double? plannedRunTime = null)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new DomainException("设备编码不能为空");

        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("设备名称不能为空");

        return new Equipment
        {
            Code = code,
            Name = name,
            Model = model,
            FactoryId = factoryId,
            WorkshopId = workshopId,
            LineId = lineId,
            InstallDate = installDate,
            Status = EquipmentStatus.IDLE,
            MaintainCycle = maintainCycle,
            TheoreticalCycleTime = theoreticalCycleTime,
            PlannedRunTime = plannedRunTime
        };
    }

    #endregion

    #region 行为方法

    /// <summary>
    /// 设置设备状态
    /// </summary>
    public void SetStatus(EquipmentStatus status)
    {
        Status = status;
    }

    /// <summary>
    /// 记录保养（更新保养日期和状态）
    /// </summary>
    public void RecordMaintenance()
    {
        LastMaintainDate = DateTime.UtcNow;
        if (MaintainCycle.HasValue && MaintainCycle.Value > 0)
            NextMaintainDate = DateTime.UtcNow.AddDays(MaintainCycle.Value);

        // 保养完成后，设备恢复运行状态
        Status = EquipmentStatus.RUNNING;
    }

    /// <summary>
    /// 报修（设置状态为故障）
    /// </summary>
    public void ReportFault()
    {
        Status = EquipmentStatus.BROKEN;
    }

    /// <summary>
    /// 添加保养计划
    /// </summary>
    public MaintenancePlan AddMaintenancePlan(string planName, int cycleDays, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(planName))
            throw new DomainException("保养计划名称不能为空");

        if (cycleDays <= 0)
            throw new DomainException("保养周期必须大于0");

        var plan = new MaintenancePlan(
            Id,
            planName,
            cycleDays,
            description);

        _maintenancePlans.Add(plan);
        return plan;
    }

    /// <summary>
    /// 设置保养周期
    /// </summary>
    public void SetMaintainCycle(int cycleDays)
    {
        if (cycleDays < 0)
            throw new DomainException("保养周期不能为负数");

        MaintainCycle = cycleDays;
    }

    /// <summary>
    /// 更新理论节拍
    /// </summary>
    public void UpdateTheoreticalCycleTime(double? cycleTime)
    {
        if (cycleTime.HasValue && cycleTime.Value < 0)
            throw new DomainException("理论节拍不能为负数");

        TheoreticalCycleTime = cycleTime;
    }

    /// <summary>
    /// 更新日计划运行时间
    /// </summary>
    public void UpdatePlannedRunTime(double? hours)
    {
        if (hours.HasValue && hours.Value < 0)
            throw new DomainException("日计划运行时间不能为负数");

        PlannedRunTime = hours;
    }

    #endregion
}