using MES.Application.Interfaces;

namespace MES.Application.Integration.Events;

public class WorkOrderCreatedEvent : IEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public long WorkOrderId { get; set; }
    public string OrderNo { get; set; } = string.Empty;
    public long MaterialId { get; set; }
    public decimal PlannedQty { get; set; }
    public string? SourceRef { get; set; }
}
