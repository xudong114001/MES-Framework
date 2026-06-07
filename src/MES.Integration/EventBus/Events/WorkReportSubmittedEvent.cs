namespace MES.Integration.EventBus.Events;

public class WorkReportSubmittedEvent : EventBase
{
    public override string EventType => "WorkReport.Submitted";
    public long WorkReportId { get; set; }
    public long WorkOrderId { get; set; }
    public string OrderNo { get; set; } = string.Empty;
    public decimal GoodQty { get; set; }
    public decimal ScrapQty { get; set; }
    public decimal ReworkQty { get; set; }
}