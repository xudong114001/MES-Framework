namespace MES.Integration.EventBus.Events;

public class WorkOrderCreatedEvent : EventBase
{
    public override string EventType => "WorkOrder.Created";
    public long WorkOrderId { get; set; }
    public string OrderNo { get; set; } = string.Empty;
    public long MaterialId { get; set; }
    public decimal PlannedQty { get; set; }
}