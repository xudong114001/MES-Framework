using MES.Application.Interfaces;
using MES.Domain.Enums;

namespace MES.Application.Integration.Events;

public class WorkOrderStatusChangedEvent : IEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public long WorkOrderId { get; set; }
    public string OrderNo { get; set; } = string.Empty;
    public WorkOrderStatus OldStatus { get; set; }
    public WorkOrderStatus NewStatus { get; set; }
    public DateTime? ChangedAt { get; set; }
    public long? ChangedBy { get; set; }
}
