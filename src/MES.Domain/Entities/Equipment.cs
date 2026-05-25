using MES.Domain.Enums;

namespace MES.Domain.Entities;

public class Equipment : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Model { get; set; }
    public long? FactoryId { get; set; }
    public long? WorkshopId { get; set; }
    public long? LineId { get; set; }
    public DateTime? InstallDate { get; set; }
    public EquipmentStatus Status { get; set; }
    public DateTime? LastMaintainDate { get; set; }
    public DateTime? NextMaintainDate { get; set; }
    public int? MaintainCycle { get; set; }

    /// <summary>
    /// 理论节拍（秒/件）
    /// </summary>
    public double? TheoreticalCycleTime { get; set; }

    /// <summary>
    /// 日计划运行时间（小时）
    /// </summary>
    public double? PlannedRunTime { get; set; }
}
