namespace MES.Integration.Models;

public class SyncRequest
{
    public string? AdapterType { get; set; }
    public string? Action { get; set; }
    public DateTime? Since { get; set; }
    public string? MaterialCode { get; set; }
}
