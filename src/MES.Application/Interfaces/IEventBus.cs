namespace MES.Application.Interfaces;

public interface IEventBus
{
    Task Publish<T>(T eventData) where T : IEvent;
    void Subscribe<T, THandler>() where T : IEvent where THandler : IEventHandler<T>;
}
