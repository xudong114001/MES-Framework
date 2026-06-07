using Microsoft.Extensions.Logging;
using NModbus;
using System.Net.Sockets;

namespace MES.Integration.Plc;

public class ModbusTcpCollector : IPlcCollector, IDisposable
{
    private readonly ILogger<ModbusTcpCollector> _logger;
    private IModbusMaster? _master;
    private TcpClient? _tcpClient;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private bool _disposed;

    public string DeviceName { get; }
    public string IpAddress { get; }
    public int Port { get; }
    public bool IsConnected => _tcpClient?.Connected ?? false;

    public ModbusTcpCollector(
        string deviceName,
        string ipAddress,
        int port,
        ILogger<ModbusTcpCollector> logger)
    {
        DeviceName = deviceName;
        IpAddress = ipAddress;
        Port = port;
        _logger = logger;
    }

    public async Task<bool> ConnectAsync(CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            if (IsConnected) return true;

            _tcpClient = new TcpClient();
            await _tcpClient.ConnectAsync(IpAddress, Port, cancellationToken);
            var factory = new ModbusFactory();
            _master = factory.CreateMaster(_tcpClient);
            _logger.LogInformation("Modbus connected to {DeviceName} at {IpAddress}:{Port}", DeviceName, IpAddress, Port);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Modbus connection failed: {DeviceName}", DeviceName);
            return false;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task DisconnectAsync()
    {
        await _lock.WaitAsync();
        try
        {
            _tcpClient?.Dispose();
            _tcpClient = null;
            _master = null;
            _logger.LogInformation("Modbus disconnected: {DeviceName}", DeviceName);
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<PlcData> ReadDataAsync(CancellationToken cancellationToken = default)
    {
        var result = new PlcData
        {
            DeviceName = DeviceName,
            IpAddress = IpAddress,
            Timestamp = DateTime.UtcNow,
            Status = IsConnected ? DeviceStatus.Running : DeviceStatus.Alarm
        };

        if (!IsConnected || _master == null)
        {
            _logger.LogWarning("Modbus not connected: {DeviceName}", DeviceName);
            return result;
        }

        cancellationToken.ThrowIfCancellationRequested();
        await _lock.WaitAsync(cancellationToken);
        try
        {
            var holding = _master.ReadHoldingRegisters(0, 0, 10);
            for (int i = 0; i < holding.Length; i++)
            {
                result.Registers[$"HR_{i}"] = holding[i];
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Modbus read failed: {DeviceName}", DeviceName);
            result.Status = DeviceStatus.Alarm;
        }
        finally
        {
            _lock.Release();
        }

        return result;
    }

    public async Task WriteDataAsync(PlcData data, CancellationToken cancellationToken = default)
    {
        if (!IsConnected || _master == null)
            return;

        cancellationToken.ThrowIfCancellationRequested();
        await _lock.WaitAsync(cancellationToken);
        try
        {
            if (data.Registers.TryGetValue("HR_0", out var value) && value is ushort u)
            {
                _master.WriteSingleRegister(0, 0, u);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Modbus write failed: {DeviceName}", DeviceName);
        }
        finally
        {
            _lock.Release();
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _tcpClient?.Dispose();
        _lock.Dispose();
        _disposed = true;
    }
}
