using MES.Domain.Enums;

namespace MES.Domain.Events;

public class WorkReportSubmittedEvent : DomainEvent
{
    public long WorkReportId { get; init; }
    public string ReportNo { get; init; } = string.Empty;
    public long WorkOrderId { get; init; }
    public decimal GoodQty { get; init; }
    public decimal ScrapQty { get; init; }
    public decimal ReworkQty { get; init; }
    public ReportType ReportType { get; init; }
    public string? BatchNo { get; init; }
}