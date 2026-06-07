using MES.Integration.Dtos;

namespace MES.Integration.Adapters;

public interface IWMSAdapter : IExternalAdapter
{
    Task<List<SyncInventoryDto>> PullInventoryAsync(string? materialCode = null, CancellationToken cancellationToken = default);
    Task PushInboundAsync(SyncInventoryDto dto, CancellationToken cancellationToken = default);
    Task PushOutboundAsync(SyncInventoryDto dto, CancellationToken cancellationToken = default);
}
