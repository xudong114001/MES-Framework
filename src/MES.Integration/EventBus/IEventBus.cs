namespace MES.Integration.EventBus;

public interface IEvent
{
    string EventId { get; }
    DateTime OccurredAt { get; }
    string EventType { get; }
}

public abstract class EventBase : IEvent
{
    public string EventId { get; } = Guid.NewGuid().ToString();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
    public abstract string EventType { get; }
}

public interface IEventBus
{
    Task PublishAsync<TEvent>(TEvent @event) where TEvent : IEvent;
    Task SubscribeAsync<TEvent>(Func<TEvent, Task> handler) where TEvent : IEvent;
}

public interface IEventHandler<TEvent> where TEvent : IEvent
{
    Task HandleAsync(TEvent @event);
}