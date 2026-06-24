using MES.Domain.Enums;

namespace MES.Domain.Events;

public class AndonEventCreatedEvent : DomainEvent
{
    public long AndonEventId { get; init; }
    public AndonEventType EventType { get; init; }
    public AndonEventLevel Level { get; init; }
    public string Title { get; init; } = string.Empty;
    public long? WorkstationId { get; init; }
    public long? WorkOrderId { get; init; }
    public long? TriggeredById { get; init; }
}
