using MES.Application.Interfaces;

namespace MES.Application.Integration.Events;

public class MaterialInventoryUpdatedEvent : IEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public long MaterialId { get; set; }
    public string MaterialCode { get; set; } = string.Empty;
    public string MaterialName { get; set; } = string.Empty;
    public decimal OldQty { get; set; }
    public decimal NewQty { get; set; }
    public decimal Diff { get; set; }
    public string? Location { get; set; }
}
