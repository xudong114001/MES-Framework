namespace MES.Integration.Models;

public class AdapterStatus
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public bool IsConnected { get; set; }
    public DateTime? LastSyncTime { get; set; }
    public string Status { get; set; } = "Ready";
    public string? Error { get; set; }
}
