using MES.Domain.Enums;

namespace MES.Domain.Events;

public class QcInspectionFailedEvent : DomainEvent
{
    public long InspectionId { get; init; }
    public string InspectNo { get; init; } = string.Empty;
    public long? WorkOrderId { get; init; }
    public long? MaterialId { get; init; }
    public QcResult InspectResult { get; init; }
    public InspectionResult? HandlingAction { get; init; }
}
