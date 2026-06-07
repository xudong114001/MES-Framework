namespace MES.Integration.Dtos;

public class SyncInventoryDto
{
    public string MaterialCode { get; set; } = string.Empty;
    public string MaterialName { get; set; } = string.Empty;
    public string? WarehouseCode { get; set; }
    public string? LocationCode { get; set; }
    public string? BatchNo { get; set; }
    public decimal Qty { get; set; }
    public string MoveType { get; set; } = string.Empty;
    public DateTime? Timestamp { get; set; }
}
