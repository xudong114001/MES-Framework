namespace MES.Application.Interfaces;

public interface IEventHandler<in TEvent> where TEvent : IEvent
{
    Task Handle(TEvent eventData, CancellationToken cancellationToken = default);
}
