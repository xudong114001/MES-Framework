using MES.Domain.Enums;

namespace MES.Application.Dtos;

/// <summary>
/// 工单 DTO — 排除导航属性，仅暴露数据传输所需字段
/// </summary>
public class WorkOrderDto
{
    public long Id { get; set; }
    public string OrderNo { get; set; } = string.Empty;
    public SourceType SourceType { get; set; }
    public string? SourceRef { get; set; }
    public long MaterialId { get; set; }
    public long? RoutingId { get; set; }
    public decimal PlannedQty { get; set; }
    public decimal CompletedQty { get; set; }
    public decimal ScrapQty { get; set; }
    public WorkOrderStatus Status { get; set; }
    public DateTime? PlanStartTime { get; set; }
    public DateTime? PlanEndTime { get; set; }
    public DateTime? ActualStartTime { get; set; }
    public DateTime? ActualEndTime { get; set; }
    public Priority Priority { get; set; }
    public long? FactoryId { get; set; }
    public long? WorkshopId { get; set; }
    public long? LineId { get; set; }
    public long? Assignee { get; set; }
    public string? Remark { get; set; }
    public long? ReworkFromId { get; set; }

    public List<WorkOrderStepDto> Steps { get; set; } = new();

    public DateTime CreatedAt { get; set; }
    public long? CreatedBy { get; set; }
    public DateTime UpdatedAt { get; set; }
    public long? UpdatedBy { get; set; }
}

/// <summary>
/// 工单工序步骤 DTO
/// </summary>
public class WorkOrderStepDto
{
    public long Id { get; set; }
    public long WorkOrderId { get; set; }
    public int StepNo { get; set; }
    public string StepName { get; set; } = string.Empty;
    public long? WorkstationId { get; set; }
    public decimal PlannedQty { get; set; }
    public decimal CompletedQty { get; set; }
    public decimal ScrapQty { get; set; }
    public WorkOrderStatus Status { get; set; }
    public DateTime? PlanStartTime { get; set; }
    public DateTime? PlanEndTime { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// 拆单请求
/// </summary>
public class SplitRequest
{
    public decimal SplitQty { get; set; }
}

/// <summary>
/// 返工请求
/// </summary>
public class ReworkRequest
{
    public decimal ReworkQty { get; set; }
    public string? Remark { get; set; }
}

/// <summary>
/// 报废请求
/// </summary>
public class ScrapRequest
{
    public decimal ScrapQty { get; set; }
    public string? Remark { get; set; }
}
