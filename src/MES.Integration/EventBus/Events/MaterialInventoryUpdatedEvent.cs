namespace MES.Integration.EventBus.Events;

public class MaterialInventoryUpdatedEvent : EventBase
{
    public override string EventType => "Material.InventoryUpdated";
    public long MaterialId { get; set; }
    public string MaterialCode { get; set; } = string.Empty;
    public decimal QuantityChange { get; set; }
    public string Direction { get; set; } = string.Empty;
}