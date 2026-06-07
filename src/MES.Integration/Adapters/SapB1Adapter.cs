using Microsoft.Extensions.Logging;
using MES.Integration.Dtos;
using MES.Integration.Models;

namespace MES.Integration.Adapters;

public class SapB1Adapter : IERPAdapter
{
    private readonly ILogger<SapB1Adapter> _logger;

    public string AdapterName => "SAP B1";
    public string AdapterVersion => "10.0";
    public bool IsConnected => false;

    public SapB1Adapter(ILogger<SapB1Adapter> logger)
    {
        _logger = logger;
    }

    public Task<bool> TestConnectionAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("SAP B1 adapter not implemented");
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
            Error = "SAP B1 适配器尚未实现"
        };
    }

    public Task<List<SyncWorkOrderDto>> PullWorkOrdersAsync(DateTime? since = null, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("SAP B1 adapter not implemented");
    }

    public Task<SyncWorkOrderDto?> GetWorkOrderAsync(string orderNo, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("SAP B1 adapter not implemented");
    }

    public Task PushWorkReportAsync(SyncWorkReportDto report, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("SAP B1 adapter not implemented");
    }

    public Task<List<SyncMaterialDto>> PullMaterialsAsync(DateTime? since = null, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("SAP B1 adapter not implemented");
    }

    public Task<List<SyncBomDto>> PullBomsAsync(DateTime? since = null, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("SAP B1 adapter not implemented");
    }
}
