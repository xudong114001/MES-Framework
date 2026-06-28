using MES.Domain.Enums;

namespace MES.Application.Dtos;

/// <summary>
/// 保养计划 DTO
/// </summary>
public class MaintenancePlanDto
{
    public long Id { get; set; }
    public long EquipmentId { get; set; }
    public string PlanName { get; set; } = string.Empty;
    public int CycleDays { get; set; }
    public DateTime? LastCompletedDate { get; set; }
    public DateTime NextDueDate { get; set; }
    public string? Description { get; set; }
    public MaintenancePlanStatus Status { get; set; }
    public string? EquipmentName { get; set; }
    public DateTime CreatedAt { get; set; }
    public long? CreatedBy { get; set; }
    public DateTime UpdatedAt { get; set; }
    public long? UpdatedBy { get; set; }
}

/// <summary>
/// 创建保养计划请求
/// </summary>
public class CreateMaintenancePlanRequest
{
    public string PlanName { get; set; } = string.Empty;
    public int CycleDays { get; set; }
    public string? Description { get; set; }
}

/// <summary>
/// 更新保养计划请求
/// </summary>
public class UpdateMaintenancePlanRequest
{
    public string PlanName { get; set; } = string.Empty;
    public int CycleDays { get; set; }
    public string? Description { get; set; }
}
