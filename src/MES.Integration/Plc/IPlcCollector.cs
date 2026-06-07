namespace MES.Integration.Plc;

public interface IPlcCollector
{
    string DeviceName { get; }
    string IpAddress { get; }
    int Port { get; }
    bool IsConnected { get; }
    Task<bool> ConnectAsync(CancellationToken cancellationToken = default);
    Task DisconnectAsync();
    Task<PlcData> ReadDataAsync(CancellationToken cancellationToken = default);
    Task WriteDataAsync(PlcData data, CancellationToken cancellationToken = default);
}
