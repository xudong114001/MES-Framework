using MES.Application.Dtos;
using MES.Domain.Entities;
using MES.Domain.Enums;

namespace MES.Application.Interfaces;

public interface IWorkOrderService
{
    Task<IEnumerable<WorkOrderDto>> GetAllAsync();
    Task<WorkOrderDto?> GetByIdAsync(long id);

    /// <summary>创建工单（DTO版本）</summary>
    Task<WorkOrderDto> CreateAsync(CreateWorkOrderRequest request);

    /// <summary>更新工单（DTO版本）</summary>
    Task UpdateAsync(long id, UpdateWorkOrderRequest request);

    Task DeleteWorkOrderAsync(long id);
    Task ReleaseWorkOrderAsync(long workOrderId);
    Task HoldWorkOrderAsync(long workOrderId);
    Task ResumeWorkOrderAsync(long workOrderId);
    Task CancelWorkOrderAsync(long workOrderId);
    Task CloseWorkOrderAsync(long workOrderId);
    Task<WorkOrderDto> SplitWorkOrderAsync(long workOrderId, decimal splitQty);
    Task<WorkOrderDto> ReworkWorkOrderAsync(long workOrderId, decimal reworkQty, string? remark);
    Task ScrapWorkOrderAsync(long workOrderId, decimal scrapQty, string? remark);
}

public class CreateWorkOrderRequest
{
    public string OrderNo { get; set; } = string.Empty;
    public SourceType SourceType { get; set; }
    public string? SourceRef { get; set; }
    public long MaterialId { get; set; }
    public long? RoutingId { get; set; }
    public decimal PlannedQty { get; set; }
    public DateTime? PlanStartTime { get; set; }
    public DateTime? PlanEndTime { get; set; }
    public Priority Priority { get; set; } = Priority.NORMAL;
    public long? FactoryId { get; set; }
    public long? WorkshopId { get; set; }
    public long? LineId { get; set; }
    public string? Assignee { get; set; }
    public string? Remark { get; set; }
}

public class UpdateWorkOrderRequest
{
    public decimal PlannedQty { get; set; }
    public DateTime? PlanStartTime { get; set; }
    public DateTime? PlanEndTime { get; set; }
    public Priority Priority { get; set; }
    public long? FactoryId { get; set; }
    public long? WorkshopId { get; set; }
    public long? LineId { get; set; }
    public string? Assignee { get; set; }
    public string? Remark { get; set; }
}

public class SplitRequest
{
    public decimal SplitQty { get; set; }
}

public class ReworkRequest
{
    public decimal ReworkQty { get; set; }
    public string? Remark { get; set; }
}

public class ScrapRequest
{
    public decimal ScrapQty { get; set; }
    public string? Remark { get; set; }
}
