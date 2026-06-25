using MES.Domain.Events;

namespace MES.Domain.Entities;

public abstract class BaseEntity
{
    public long Id { get; protected set; }

    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;
    public long? CreatedBy { get; protected set; }

    public DateTime UpdatedAt { get; protected set; } = DateTime.UtcNow;
    public long? UpdatedBy { get; protected set; }

    public bool IsDeleted { get; protected set; }

    /// <summary>
    /// 标记为已删除（软删除）
    /// </summary>
    public void MarkAsDeleted()
    {
        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// 恢复已删除记录
    /// </summary>
    public void MarkAsUndeleted()
    {
        IsDeleted = false;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// 设置创建信息（供 DbContext SaveChanges 自动填充）
    /// </summary>
    public void SetCreationInfo(DateTime createdAt, long? createdBy = null)
    {
        CreatedAt = createdAt;
        CreatedBy = createdBy;
    }

    /// <summary>
    /// 设置修改信息（供 DbContext SaveChanges 自动填充）
    /// </summary>
    public void SetModificationInfo(DateTime updatedAt, long? updatedBy = null)
    {
        UpdatedAt = updatedAt;
        UpdatedBy = updatedBy;
    }

    /// <summary>
    /// 设置创建人
    /// </summary>
    protected void SetCreatedBy(long? userId) => CreatedBy = userId;

    /// <summary>
    /// 设置修改人
    /// </summary>
    protected void SetUpdatedBy(long? userId) => UpdatedBy = userId;

    private readonly List<DomainEvent> _domainEvents = new();
    public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void AddDomainEvent(DomainEvent eventItem) => _domainEvents.Add(eventItem);
    protected void RemoveDomainEvent(DomainEvent eventItem) => _domainEvents.Remove(eventItem);
    public void ClearDomainEvents() => _domainEvents.Clear();
}
