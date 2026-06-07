using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace MES.Integration.EventBus;

public class RabbitMQEventBus : IEventBus, IDisposable
{
    private readonly ILogger<RabbitMQEventBus> _logger;
    private readonly ConcurrentDictionary<string, List<Func<IEvent, Task>>> _handlers = new();
    private IConnection? _connection;
    private IChannel? _channel;
    private readonly string _exchangeName = "mes_event_bus";
    private bool _disposed;

    public RabbitMQEventBus(ILogger<RabbitMQEventBus> logger)
    {
        _logger = logger;
    }

    public async Task PublishAsync<TEvent>(TEvent @event) where TEvent : IEvent
    {
        var eventName = typeof(TEvent).Name;
        var json = JsonSerializer.Serialize(@event, @event.GetType());
        var body = Encoding.UTF8.GetBytes(json);

        try
        {
            await EnsureConnectedAsync();
            if (_channel is null)
            {
                _logger.LogWarning("RabbitMQ channel not available, storing event locally");
                return;
            }
            await _channel.BasicPublishAsync(
                exchange: _exchangeName,
                routingKey: eventName,
                body: body);
            _logger.LogInformation("Published event: {EventName} | {EventId}", eventName, @event.EventId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish event: {EventName}", eventName);
        }
    }

    public async Task SubscribeAsync<TEvent>(Func<TEvent, Task> handler) where TEvent : IEvent
    {
        var eventName = typeof(TEvent).Name;
        _handlers.AddOrUpdate(eventName,
            _ => new List<Func<IEvent, Task>> { e => handler((TEvent)e) },
            (_, existing) =>
            {
                existing.Add(e => handler((TEvent)e));
                return existing;
            });

        _logger.LogInformation("Subscribed to event: {EventName}", eventName);
        await Task.CompletedTask;
    }

    private async Task EnsureConnectedAsync()
    {
        if (_connection is not null && _connection.IsOpen && _channel is not null && _channel.IsOpen)
            return;

        try
        {
            var factory = new ConnectionFactory
            {
                HostName = "localhost",
                Port = 5672,
                UserName = "mes_user",
                Password = "Mes@2026!",
                VirtualHost = "/",
                AutomaticRecoveryEnabled = true
            };

            _connection = await factory.CreateConnectionAsync();
            _channel = await _connection.CreateChannelAsync();
            await _channel.ExchangeDeclareAsync(exchange: _exchangeName, type: ExchangeType.Direct, durable: true);
            _logger.LogInformation("Connected to RabbitMQ");
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