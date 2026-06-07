namespace MES.Integration.Dtos;

public class SyncBomDto
{
    public string ProductCode { get; set; } = string.Empty;
    public string? Version { get; set; }
    public List<SyncBomItemDto> Items { get; set; } = new();
}

public class SyncBomItemDto
{
    public string MaterialCode { get; set; } = string.Empty;
    public decimal Qty { get; set; }
    public string? Unit { get; set; }
    public int? Sequence { get; set; }
    public bool IsKeyPart { get; set; }
}
