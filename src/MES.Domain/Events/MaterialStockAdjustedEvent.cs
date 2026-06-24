namespace MES.Domain.Events;

public class MaterialStockAdjustedEvent : DomainEvent
{
    public long MaterialId { get; init; }
    public string MaterialCode { get; init; } = string.Empty;
    public string MaterialName { get; init; } = string.Empty;
    public decimal OldStockQty { get; init; }
    public decimal NewStockQty { get; init; }
    public decimal AdjustDiff { get; init; }
}
