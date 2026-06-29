using System.Collections.Concurrent;
using MES.Application.Interfaces;

namespace MES.Integration.EventBus;

public class SubscriptionManager
{
    private readonly ConcurrentDictionary<string, List<Type>> _subscriptions = new();

    public void AddSubscription<TEvent, THandler>() where TEvent : IEvent
    {
        var eventName = typeof(TEvent).Name;
        _subscriptions.AddOrUpdate(eventName,
            _ => new List<Type> { typeof(THandler) },
            (_, existing) =>
            {
                if (!existing.Contains(typeof(THandler)))
                    existing.Add(typeof(THandler));
                return existing;
            });
    }

    public List<Type> GetHandlers(string eventName)
    {
        return _subscriptions.TryGetValue(eventName, out var handlers) ? handlers : new List<Type>();
    }
}
