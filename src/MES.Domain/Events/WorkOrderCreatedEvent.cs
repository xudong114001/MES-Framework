namespace MES.Domain.Events;

public class WorkOrderCreatedEvent : DomainEvent
{
    public long WorkOrderId { get; init; }
    public string OrderNo { get; init; } = string.Empty;
    public long MaterialId { get; init; }
    public decimal PlannedQty { get; init; }
    public string? SourceRef { get; init; }
}