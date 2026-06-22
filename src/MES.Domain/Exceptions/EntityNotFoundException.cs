namespace MES.Domain.Exceptions;

public class EntityNotFoundException : DomainException
{
    public string EntityType { get; }
    public long? EntityId { get; }

    public EntityNotFoundException(string entityType, long entityId)
        : base($"{entityType} (Id={entityId}) 不存在", "NOT_FOUND")
    {
        EntityType = entityType;
        EntityId = entityId;
    }

    public EntityNotFoundException(string message)
        : base(message, "NOT_FOUND")
    {
        EntityType = string.Empty;
        EntityId = null;
    }
}