using MES.Integration.Models;

namespace MES.Integration.Adapters;

public interface IExternalAdapter
{
    string AdapterName { get; }
    string AdapterVersion { get; }
    bool IsConnected { get; }
    Task<bool> TestConnectionAsync(CancellationToken cancellationToken = default);
    AdapterStatus GetStatus();
}
