using MES.Domain.Enums;

namespace MES.Application.Dtos;

/// <summary>
/// 物料 DTO
/// </summary>
public class MaterialDto
{
    public long Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Spec { get; set; }
    public string? Unit { get; set; }
    public string? Category { get; set; }
    public int? BomLevel { get; set; }
    public decimal StockQty { get; set; }
    public bool Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public long? CreatedBy { get; set; }
    public DateTime UpdatedAt { get; set; }
    public long? UpdatedBy { get; set; }
}

/// <summary>
/// 设备 DTO
/// </summary>
public class EquipmentDto
{
    public long Id { get; set; }
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
    public double? TheoreticalCycleTime { get; set; }
    public double? PlannedRunTime { get; set; }
    public DateTime CreatedAt { get; set; }
    public long? CreatedBy { get; set; }
    public DateTime UpdatedAt { get; set; }
    public long? UpdatedBy { get; set; }
}

/// <summary>
/// 工艺路线 DTO
/// </summary>
public class RoutingDto
{
    public long Id { get; set; }
    public long MaterialId { get; set; }
    public string RoutingCode { get; set; } = string.Empty;
    public string RoutingName { get; set; } = string.Empty;
    public string Version { get; set; } = "V1.0";
    public bool Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public long? CreatedBy { get; set; }
    public DateTime UpdatedAt { get; set; }
    public long? UpdatedBy { get; set; }
}

/// <summary>
/// 工序 DTO
/// </summary>
public class RoutingStepDto
{
    public long Id { get; set; }
    public long RoutingId { get; set; }
    public int StepNo { get; set; }
    public string StepName { get; set; } = string.Empty;
    public string? WorkstationType { get; set; }
    public decimal StandardTime { get; set; }
    public bool IsQcPoint { get; set; }
    public bool IsScrapPoint { get; set; }
    public DateTime CreatedAt { get; set; }
    public long? CreatedBy { get; set; }
    public DateTime UpdatedAt { get; set; }
    public long? UpdatedBy { get; set; }
}

/// <summary>
/// BOM DTO
/// </summary>
public class BomDto
{
    public long Id { get; set; }
    public long ProductId { get; set; }
    public long MaterialId { get; set; }
    public decimal Quantity { get; set; }
    public decimal ScrapRate { get; set; }
    public int SeqNo { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public bool Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public long? CreatedBy { get; set; }
    public DateTime UpdatedAt { get; set; }
    public long? UpdatedBy { get; set; }
}