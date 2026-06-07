namespace MES.Integration.Dtos;

public class SyncMaterialDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Spec { get; set; }
    public string? Unit { get; set; }
    public string? Category { get; set; }
    public int? BomLevel { get; set; }
    public bool Status { get; set; } = true;
}
