namespace MES.Integration.EventBus.Events;

public class WorkOrderStatusChangedEvent : EventBase
{
    public override string EventType => "WorkOrder.StatusChanged";
    public long WorkOrderId { get; set; }
    public string OrderNo { get; set; } = string.Empty;
    public string OldStatus { get; set; } = string.Empty;
    public string NewStatus { get; set; } = string.Empty;
}