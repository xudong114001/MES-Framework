namespace MES.Domain.Events;

public class MaterialInventoryUpdatedEvent : DomainEvent
{
    public long MaterialId { get; init; }
    public string MaterialCode { get; init; } = string.Empty;
    public string MaterialName { get; init; } = string.Empty;
    public decimal OldQty { get; init; }
    public decimal NewQty { get; init; }
    public decimal Diff { get; init; }
    public string? Location { get; init; }
}