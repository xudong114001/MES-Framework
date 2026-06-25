using MES.Application.Interfaces;
using MES.Domain.Enums;

namespace MES.Application.Integration.Events;

public class QcInspectionCompletedEvent : IEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public long InspectionId { get; set; }
    public string InspectNo { get; set; } = string.Empty;
    public long? WorkOrderId { get; set; }
    public long? MaterialId { get; set; }
    public QcResult Result { get; set; }
    public InspectionResult? HandlingAction { get; set; }
    public DateTime? HandledAt { get; set; }
}
