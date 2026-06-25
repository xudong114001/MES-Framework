using MES.Domain.Enums;

namespace MES.Domain.Events;

public class QcInspectionCompletedEvent : DomainEvent
{
    public long InspectionId { get; init; }
    public string InspectNo { get; init; } = string.Empty;
    public long? WorkOrderId { get; init; }
    public long? MaterialId { get; init; }
    public QcResult Result { get; init; }
    public InspectionResult? HandlingAction { get; init; }
    public DateTime? HandledAt { get; init; }
}
