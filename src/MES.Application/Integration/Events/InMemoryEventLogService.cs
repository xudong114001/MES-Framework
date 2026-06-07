using System.Collections.Concurrent;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using MES.Application.Interfaces;

namespace MES.Application.Integration.Events;

public class EventLogEntry
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string EventName { get; set; } = string.Empty;
    public Guid EventId { get; set; }
    public string Payload { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = string.Empty;
    public string? Error { get; set; }
}

public class InMemoryEventLogService
{
    private readonly ConcurrentBag<EventLogEntry> _logs = new();
    private readonly ILogger<InMemoryEventLogService>? _logger;

    public InMemoryEventLogService(ILogger<InMemoryEventLogService>? logger = null)
    {
        _logger = logger;
    }

    public void Log<T>(T eventData, string status, string? error = null) where T : IEvent
    {
        var entry = new EventLogEntry
        {
            EventName = typeof(T).Name,
            EventId = eventData.Id,
            Payload = JsonSerializer.Serialize(eventData),
            Status = status,
            Error = error,
            Timestamp = DateTime.UtcNow
        };
        _logs.Add(entry);
        _logger?.LogDebug("[EventLog] {EventName} ({Status}) at {Timestamp}", entry.EventName, status, entry.Timestamp);
    }

    public IReadOnlyCollection<EventLogEntry> GetLogs()
    {
        return _logs.OrderByDescending(x => x.Timestamp).ToList();
    }

    public void Clear()
    {
        while (_logs.TryTake(out _)) { }
    }
}
