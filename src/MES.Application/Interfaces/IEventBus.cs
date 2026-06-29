namespace MES.Application.Interfaces;

public interface IEventBus
{
    Task PublishAsync<T>(T eventData) where T : IEvent;
}
