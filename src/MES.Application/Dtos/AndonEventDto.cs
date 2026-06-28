using MES.Domain.Enums;

namespace MES.Application.Dtos;

/// <summary>
/// Andon 异常事件 DTO
/// </summary>
public class AndonEventDto
{
    public long Id { get; set; }
    public AndonEventType EventType { get; set; }
    public AndonEventLevel Level { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public long? WorkstationId { get; set; }
    public string? WorkstationName { get; set; }
    public long? WorkOrderId { get; set; }
    public string? WorkOrderNo { get; set; }
    public long? TriggeredById { get; set; }
    public string? TriggeredByName { get; set; }
    public DateTime TriggeredAt { get; set; }
    public long? ResolvedById { get; set; }
    public string? ResolvedByName { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// 触发 Andon 事件请求
/// </summary>
public class TriggerAndonRequest
{
    public AndonEventType EventType { get; set; }
    public AndonEventLevel? Level { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public long? WorkstationId { get; set; }
    public string? WorkstationName { get; set; }
    public long? WorkOrderId { get; set; }
    public string? WorkOrderNo { get; set; }
}

/// <summary>
/// 处理 Andon 事件请求
/// </summary>
public class ResolveAndonRequest
{
    public string? Handler { get; set; }
}
