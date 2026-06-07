using Microsoft.Extensions.Logging;

namespace MES.Integration.Plc;

public class MockPlcCollector : IPlcCollector
{
    private readonly ILogger<MockPlcCollector> _logger;
    private bool _connected;
    private readonly Random _random = new();

    public string DeviceName { get; }
    public string IpAddress { get; }
    public int Port { get; }
    public bool IsConnected => _connected;

    public MockPlcCollector(ILogger<MockPlcCollector> logger)
    {
        DeviceName = "MockPLC";
        IpAddress = "127.0.0.1";
        Port = 502;
        _logger = logger;
    }

    public Task<bool> ConnectAsync(CancellationToken cancellationToken = default)
    {
        _connected = true;
        _logger.LogInformation("MockPLC connected: {DeviceName}", DeviceName);
        return Task.FromResult(true);
    }

    public Task DisconnectAsync()
    {
        _connected = false;
        _logger.LogInformation("MockPLC disconnected: {DeviceName}", DeviceName);
        return Task.CompletedTask;
    }

    public Task<PlcData> ReadDataAsync(CancellationToken cancellationToken = default)
    {
        var data = new PlcData
        {
            DeviceName = DeviceName,
            IpAddress = IpAddress,
            Timestamp = DateTime.UtcNow,
            Status = DeviceStatus.Running,
            Registers = new Dictionary<string, object>
            {
                { "HR_0", (ushort)_random.Next(0, 100) },
                { "HR_1", (ushort)_random.Next(0, 1000) },
                { "HR_2", (ushort)_random.Next(0, 2) }
            }
        };
        return Task.FromResult(data);
    }

    public Task WriteDataAsync(PlcData data, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("MockPLC write: {DeviceName} registers={Count}", DeviceName, data.Registers.Count);
        return Task.CompletedTask;
    }
}
