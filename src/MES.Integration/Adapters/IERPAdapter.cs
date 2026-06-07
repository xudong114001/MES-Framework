using MES.Integration.Dtos;

namespace MES.Integration.Adapters;

public interface IERPAdapter : IExternalAdapter
{
    Task<List<SyncWorkOrderDto>> PullWorkOrdersAsync(DateTime? since = null, CancellationToken cancellationToken = default);
    Task<SyncWorkOrderDto?> GetWorkOrderAsync(string orderNo, CancellationToken cancellationToken = default);
    Task PushWorkReportAsync(SyncWorkReportDto report, CancellationToken cancellationToken = default);
    Task<List<SyncMaterialDto>> PullMaterialsAsync(DateTime? since = null, CancellationToken cancellationToken = default);
    Task<List<SyncBomDto>> PullBomsAsync(DateTime? since = null, CancellationToken cancellationToken = default);
}
