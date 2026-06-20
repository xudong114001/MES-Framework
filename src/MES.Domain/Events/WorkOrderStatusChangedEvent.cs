using MES.Domain.Enums;

namespace MES.Domain.Events;

public class WorkOrderStatusChangedEvent : DomainEvent
{
    public long WorkOrderId { get; init; }
    public string OrderNo { get; init; } = string.Empty;
    public WorkOrderStatus OldStatus { get; init; }
    public WorkOrderStatus NewStatus { get; init; }
    public DateTime? ChangedAt { get; init; }
    public long? ChangedBy { get; init; }
}