namespace MES.Application.Interfaces;

public interface IEvent
{
    Guid Id { get; }
    DateTime CreatedAt { get; }
}
