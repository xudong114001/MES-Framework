using MES.Domain.AggregateRoots;
using MES.Domain.Enums;
using MES.Domain.Events;
using MES.Domain.Exceptions;
using MES.Domain.ValueObjects;

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
        var oldStatus = Status;
        Status = EquipmentStatus.BROKEN;
        AddDomainEvent(new EquipmentFaultEvent(Id, Code, Name, oldStatus));
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

    /// <summary>
    /// 更新设备信息
    /// </summary>
    public void Update(
        string name,
        string? model = null,
        long? factoryId = null,
        long? workshopId = null,
        long? lineId = null,
        DateTime? installDate = null,
        EquipmentStatus? status = null,
        int? maintainCycle = null,
        double? theoreticalCycleTime = null,
        double? plannedRunTime = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("设备名称不能为空");

        Name = name;
        Model = model;
        FactoryId = factoryId;
        WorkshopId = workshopId;
        LineId = lineId;
        InstallDate = installDate;
        if (status.HasValue)
            Status = status.Value;
        MaintainCycle = maintainCycle;
        TheoreticalCycleTime = theoreticalCycleTime;
        PlannedRunTime = plannedRunTime;
    }

    #endregion

    #region OEE 计算静态方法

    /// <summary>
    /// 计算 OEE 综合指标
    /// OEE = 可用率(Availability) × 性能率(Performance) × 良品率(Quality)
    /// </summary>
    /// <param name="availability">可用率 (0~1)</param>
    /// <param name="performance">性能率 (0~1)</param>
    /// <param name="quality">良品率 (0~1)</param>
    /// <returns>OeeResult 值对象</returns>
    public static OeeResult CalculateOee(decimal availability, decimal performance, decimal quality)
    {
        return new OeeResult(availability, performance, quality);
    }

    /// <summary>
    /// 计算可用率
    /// Availability = 实际运行时间 / 计划运行时间
    /// </summary>
    /// <param name="actualRunTime">实际运行时间（小时）</param>
    /// <param name="plannedRunTime">计划运行时间（小时）</param>
    /// <returns>可用率 (0~1)</returns>
    public static decimal CalculateAvailability(decimal actualRunTime, decimal plannedRunTime)
    {
        if (plannedRunTime <= 0)
            throw new DomainException("计划运行时间必须大于0");

        if (actualRunTime < 0)
            throw new DomainException("实际运行时间不能为负数");

        return Math.Min(actualRunTime / plannedRunTime, 1m);
    }

    /// <summary>
    /// 计算性能率
    /// Performance = 理论节拍 × 产出数量 / 实际运行时间
    /// </summary>
    /// <param name="theoreticalCycleTime">理论节拍（秒/件）</param>
    /// <param name="outputCount">产出数量</param>
    /// <param name="actualRunTimeSeconds">实际运行时间（秒）</param>
    /// <returns>性能率 (0~1)</returns>
    public static decimal CalculatePerformance(decimal theoreticalCycleTime, decimal outputCount, decimal actualRunTimeSeconds)
    {
        if (theoreticalCycleTime <= 0)
            throw new DomainException("理论节拍必须大于0");

        if (outputCount < 0)
            throw new DomainException("产出数量不能为负数");

        if (actualRunTimeSeconds <= 0)
            throw new DomainException("实际运行时间必须大于0");

        return Math.Min((theoreticalCycleTime * outputCount) / actualRunTimeSeconds, 1m);
    }

    /// <summary>
    /// 计算良品率
    /// Quality = 良品数量 / 总产出数量
    /// </summary>
    /// <param name="goodCount">良品数量</param>
    /// <param name="totalCount">总产出数量（良品 + 不良品）</param>
    /// <returns>良品率 (0~1)</returns>
    public static decimal CalculateQuality(decimal goodCount, decimal totalCount)
    {
        if (totalCount <= 0)
            throw new DomainException("总产出数量必须大于0");

        if (goodCount < 0)
            throw new DomainException("良品数量不能为负数");

        if (goodCount > totalCount)
            throw new DomainException("良品数量不能超过总产出数量");

        return goodCount / totalCount;
    }

    #endregion
}
