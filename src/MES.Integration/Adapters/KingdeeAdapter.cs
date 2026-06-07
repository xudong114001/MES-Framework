using Microsoft.Extensions.Logging;
using MES.Integration.Dtos;
using MES.Integration.Models;

namespace MES.Integration.Adapters;

public class KingdeeAdapter : IERPAdapter
{
    private readonly ILogger<KingdeeAdapter> _logger;

    public string AdapterName => "Kingdee";
    public string AdapterVersion => "8.0";
    public bool IsConnected => false;

    public KingdeeAdapter(ILogger<KingdeeAdapter> logger)
    {
        _logger = logger;
    }

    public Task<bool> TestConnectionAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("Kingdee adapter not implemented");
        return Task.FromResult(false);
    }

    public AdapterStatus GetStatus()
    {
        return new AdapterStatus
        {
            Name = AdapterName,
            Type = "ERP",
            IsConnected = IsConnected,
            LastSyncTime = null,
            Status = "NotConfigured",
            Error = "金蝶适配器尚未实现"
        };
    }

    public Task<List<SyncWorkOrderDto>> PullWorkOrdersAsync(DateTime? since = null, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Kingdee adapter not implemented");
    }

    public Task<SyncWorkOrderDto?> GetWorkOrderAsync(string orderNo, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Kingdee adapter not implemented");
    }

    public Task PushWorkReportAsync(SyncWorkReportDto report, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Kingdee adapter not implemented");
    }

    public Task<List<SyncMaterialDto>> PullMaterialsAsync(DateTime? since = null, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Kingdee adapter not implemented");
    }

    public Task<List<SyncBomDto>> PullBomsAsync(DateTime? since = null, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Kingdee adapter not implemented");
    }
}
