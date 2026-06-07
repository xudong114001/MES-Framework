namespace MES.Integration.EventBus.Events;

public class QcInspectionCompletedEvent : EventBase
{
    public override string EventType => "QcInspection.Completed";
    public long InspectionId { get; set; }
    public string InspectNo { get; set; } = string.Empty;
    public string Result { get; set; } = string.Empty;
    public long? WorkOrderId { get; set; }
}