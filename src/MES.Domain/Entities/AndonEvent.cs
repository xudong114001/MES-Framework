using MES.Domain.AggregateRoots;
using MES.Domain.Enums;
using MES.Domain.Exceptions;

namespace MES.Domain.Entities;

public class AndonEvent : BaseEntity, IAggregateRoot
{
    /// <summary>事件类型</summary>
    public AndonEventType EventType { get; set; }

    /// <summary>事件级别</summary>
    public AndonEventLevel Level { get; set; }

    /// <summary>标题</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>详细描述</summary>
    public string? Description { get; set; }

    /// <summary>触发人工位</summary>
    public long? WorkstationId { get; set; }

    /// <summary>触发人工位名称</summary>
    public string? WorkstationName { get; set; }

    /// <summary>关联的工单ID</summary>
    public long? WorkOrderId { get; set; }

    /// <summary>关联的工单号</summary>
    public string? WorkOrderNo { get; set; }

    /// <summary>触发人ID</summary>
    public long? TriggeredById { get; set; }

    /// <summary>触发人用户名</summary>
    public string? TriggeredByName { get; set; }

    /// <summary>触发时间</summary>
    public DateTime TriggeredAt { get; set; } = DateTime.UtcNow;

    /// <summary>处理人ID</summary>
    public long? ResolvedById { get; set; }

    /// <summary>处理人用户名</summary>
    public string? ResolvedByName { get; set; }

    /// <summary>处理时间</summary>
    public DateTime? ResolvedAt { get; set; }

    /// <summary>
    /// 处理/解决 Andon 事件
    /// </summary>
    public void Resolve(long resolvedById, string resolvedByName)
    {
        if (ResolvedAt.HasValue)
            throw new DomainException("该事件已经被处理", "ANDON_ALREADY_RESOLVED");
        if (resolvedById <= 0)
            throw new DomainException("处理人ID无效", "ANDON_INVALID_RESOLVER");
        if (string.IsNullOrWhiteSpace(resolvedByName))
            throw new DomainException("处理人姓名不能为空", "ANDON_RESOLVER_NAME_REQUIRED");

        ResolvedById = resolvedById;
        ResolvedByName = resolvedByName;
        ResolvedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// 升级 Andon 事件级别
    /// </summary>
    public void Escalate()
    {
        if (ResolvedAt.HasValue)
            throw new DomainException("已处理的事件不允许升级", "ANDON_RESOLVED_CANNOT_ESCALATE");
        if (Level == AndonEventLevel.Critical)
            throw new DomainException("事件已达到最高级别，无法继续升级", "ANDON_MAX_LEVEL");

        Level = (AndonEventLevel)((int)Level + 1);
        UpdatedAt = DateTime.UtcNow;
    }
}
