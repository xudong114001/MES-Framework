using MES.Application.Interfaces;
using MES.Domain.Enums;

namespace MES.Application.Integration.Events;

public class WorkReportSubmittedEvent : IEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public long WorkReportId { get; set; }
    public string ReportNo { get; set; } = string.Empty;
    public long WorkOrderId { get; set; }
    public decimal GoodQty { get; set; }
    public decimal ScrapQty { get; set; }
    public decimal ReworkQty { get; set; }
    public ReportType ReportType { get; set; }
    public string? BatchNo { get; set; }
}
