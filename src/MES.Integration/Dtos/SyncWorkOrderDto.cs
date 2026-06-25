using MES.Domain.Enums;

namespace MES.Integration.Dtos;

public class SyncWorkOrderDto
{
    public string OrderNo { get; set; } = string.Empty;
    public string? SourceRef { get; set; }
    public string MaterialCode { get; set; } = string.Empty;
    public string? RoutingCode { get; set; }
    public decimal PlannedQty { get; set; }
    public DateTime? PlanStartTime { get; set; }
    public DateTime? PlanEndTime { get; set; }
    public WorkOrderStatus Status { get; set; }
    public Priority Priority { get; set; }
    public string? FactoryCode { get; set; }
    public string? WorkshopCode { get; set; }
    public string? LineCode { get; set; }
    public string? Remark { get; set; }
}
