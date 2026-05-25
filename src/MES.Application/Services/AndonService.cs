namespace MES.Application.Services;

/// <summary>
/// Andon 异常事件类型
/// </summary>
public enum AndonEventType
{
    QUALITY_ALARM,     // 质量异常
    EQUIPMENT_FAULT,   // 设备故障
    MATERIAL_SHORTAGE, // 物料短缺
    OTHER              // 其他异常
}

/// <summary>
/// Andon 异常事件
/// </summary>
public class AndonEvent
{
    public long Id { get; set; }
    public AndonEventType EventType { get; set; }
    public string? EventTypeName => EventType.ToString();
    public string? WorkstationName { get; set; }
    public string? Description { get; set; }
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    public string? Handler { get; set; }
    public DateTime? HandledAt { get; set; }
    public bool IsHandled => HandledAt.HasValue;
}

/// <summary>
/// Andon 异常管理服务（内存存储，后续可改为数据库）
/// </summary>
public class AndonService
{
    private readonly List<AndonEvent> _events = new();
    private long _nextId = 1;

    /// <summary>获取所有活跃的（未处理）异常事件</summary>
    public Task<IEnumerable<AndonEvent>> GetActiveEventsAsync()
    {
        var active = _events.Where(e => !e.HandledAt.HasValue)
                            .OrderByDescending(e => e.OccurredAt)
                            .AsEnumerable();
        return Task.FromResult(active);
    }

    /// <summary>获取所有异常事件</summary>
    public Task<IEnumerable<AndonEvent>> GetAllEventsAsync()
    {
        return Task.FromResult(_events.OrderByDescending(e => e.OccurredAt).AsEnumerable());
    }

    /// <summary>触发一个新的异常事件</summary>
    public Task<AndonEvent> TriggerEventAsync(AndonEventType eventType, string? workstation, string? description)
    {
        var evt = new AndonEvent
        {
            Id = _nextId++,
            EventType = eventType,
            WorkstationName = workstation,
            Description = description,
            OccurredAt = DateTime.UtcNow
        };
        _events.Add(evt);
        return Task.FromResult(evt);
    }

    /// <summary>处理/解决异常事件</summary>
    public Task ResolveEventAsync(long eventId, string? handler)
    {
        var evt = _events.FirstOrDefault(e => e.Id == eventId);
        if (evt != null)
        {
            evt.Handler = handler;
            evt.HandledAt = DateTime.UtcNow;
        }
        return Task.CompletedTask;
    }
}
