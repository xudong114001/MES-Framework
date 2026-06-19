namespace MES.Domain.Entities;

/// <summary>
/// Andon 异常事件 - 持久化到数据库
/// </summary>
public class AndonEvent : BaseEntity
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
}

/// <summary>
/// Andon 事件类型
/// </summary>
public enum AndonEventType
{
    QUALITY_ALARM,     // 质量异常
    EQUIPMENT_FAULT,   // 设备故障
    MATERIAL_SHORTAGE, // 物料短缺
    PRODUCTION_DELAY,  // 生产延迟
    OTHER              // 其他异常
}

/// <summary>
/// Andon 事件级别
/// </summary>
public enum AndonEventLevel
{
    Info,       // 提示
    Warning,    // 警告
    Error,      // 错误
    Critical    // 紧急
}