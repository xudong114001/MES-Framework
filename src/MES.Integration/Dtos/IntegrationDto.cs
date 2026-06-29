namespace MES.Integration.Dtos;

public class SyncAdapterRequest
{
    public string? Direction { get; set; }
    public DateTime? Since { get; set; }
}

public class IntegrationSyncRequest
{
    public DateTime? Since { get; set; }
    public string? MaterialCode { get; set; }
}
