namespace MES.Application.Interfaces;

public interface IEvent
{
    Guid Id { get; }
    DateTime CreatedAt { get; }
    string EventId { get; }
    string EventType { get; }
}
