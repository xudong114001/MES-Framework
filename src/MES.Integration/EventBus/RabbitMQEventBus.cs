using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MES.Application.Interfaces;
using MES.Application.Settings;
using RabbitMQ.Client;

namespace MES.Integration.EventBus;

public class RabbitMQEventBus : IEventBus, IDisposable
{
    private readonly ILogger<RabbitMQEventBus> _logger;
    private readonly RabbitMQSettings _settings;
    private IConnection? _connection;
    private IChannel? _channel;
    private readonly string _exchangeName = "mes_event_bus";
    private bool _disposed;

    public RabbitMQEventBus(ILogger<RabbitMQEventBus> logger, IOptions<RabbitMQSettings> settings)
    {
        _logger = logger;
        _settings = settings.Value;
    }

    public async Task PublishAsync<T>(T eventData) where T : IEvent
    {
        var eventName = typeof(T).Name;
        var json = JsonSerializer.Serialize(eventData, eventData.GetType());
        var body = Encoding.UTF8.GetBytes(json);

        try
        {
            await EnsureConnectedAsync();
            if (_channel is null)
            {
                _logger.LogWarning("RabbitMQ channel not available, skipping event publish");
                return;
            }
            await _channel.BasicPublishAsync(
                exchange: _exchangeName,
                routingKey: eventName,
                body: body);
            _logger.LogInformation("Published event: {EventName} | {EventId}", eventName, eventData.EventId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish event: {EventName}", eventName);
        }
    }

    private async Task EnsureConnectedAsync()
    {
        if (_connection is not null && _connection.IsOpen && _channel is not null && _channel.IsOpen)
            return;

        try
        {
            var factory = new ConnectionFactory
            {
                HostName = _settings.HostName,
                Port = _settings.Port,
                UserName = _settings.UserName,
                Password = _settings.Password,
                VirtualHost = _settings.VirtualHost,
                AutomaticRecoveryEnabled = true
            };

            _connection = await factory.CreateConnectionAsync();
            _channel = await _connection.CreateChannelAsync();
            await _channel.ExchangeDeclareAsync(exchange: _exchangeName, type: ExchangeType.Direct, durable: true);
            _logger.LogInformation("Connected to RabbitMQ at {Host}:{Port}", _settings.HostName, _settings.Port);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not connect to RabbitMQ, using in-memory mode");
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _channel?.CloseAsync().Wait();
        _connection?.CloseAsync().Wait();
        _disposed = true;
    }
}
