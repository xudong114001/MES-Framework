using Microsoft.EntityFrameworkCore;
using MES.Application.Interfaces;
using MES.Domain.Entities;
using MES.Domain.Repositories;

namespace MES.Application.Services;

/// <summary>
/// Andon 异常管理服务（数据库持久化）
/// </summary>
public class AndonService : IAndonService
{
    private readonly IRepository<AndonEvent> _repository;

    public AndonService(IRepository<AndonEvent> repository)
    {
        _repository = repository;
    }

    /// <summary>获取所有活跃的（未处理）异常事件</summary>
    public async Task<IEnumerable<AndonEvent>> GetActiveEventsAsync()
    {
        return await _repository.FindAsync(e => e.ResolvedAt == null);
    }

    /// <summary>获取所有异常事件</summary>
    public async Task<IEnumerable<AndonEvent>> GetAllEventsAsync()
    {
        var all = await _repository.FindAsync(e => true);
        return all.OrderByDescending(e => e.TriggeredAt);
    }

    /// <summary>获取异常事件（分页）</summary>
    public async Task<(IEnumerable<AndonEvent> Items, int Total)> GetEventsAsync(
        int page = 1,
        int pageSize = 20,
        bool? isResolved = null,
        Domain.Entities.AndonEventType? eventType = null)
    {
        var all = await _repository.FindAsync(e => true);

        var filtered = all.AsEnumerable();

        if (isResolved.HasValue)
        {
            filtered = isResolved.Value
                ? filtered.Where(e => e.ResolvedAt != null)
                : filtered.Where(e => e.ResolvedAt == null);
        }

        if (eventType.HasValue)
        {
            filtered = filtered.Where(e => e.EventType == eventType.Value);
        }

        var total = filtered.Count();
        var items = filtered
            .OrderByDescending(e => e.TriggeredAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return (items, total);
    }

    /// <summary>触发一个新的异常事件</summary>
    public async Task<AndonEvent> TriggerEventAsync(
        Domain.Entities.AndonEventType eventType,
        Domain.Entities.AndonEventLevel level,
        string title,
        string? description = null,
        long? workstationId = null,
        string? workstationName = null,
        long? workOrderId = null,
        string? workOrderNo = null,
        long? triggeredById = null,
        string? triggeredByName = null)
    {
        var evt = new AndonEvent
        {
            EventType = eventType,
            Level = level,
            Title = title,
            Description = description,
            WorkstationId = workstationId,
            WorkstationName = workstationName,
            WorkOrderId = workOrderId,
            WorkOrderNo = workOrderNo,
            TriggeredById = triggeredById,
            TriggeredByName = triggeredByName,
            TriggeredAt = DateTime.UtcNow
        };

        await _repository.AddAsync(evt);
        return evt;
    }

    /// <summary>处理/解决异常事件</summary>
    public async Task<bool> ResolveEventAsync(long eventId, long resolverId, string resolverName)
    {
        var evt = await _repository.GetByIdAsync(eventId);
        if (evt == null) return false;

        evt.ResolvedById = resolverId;
        evt.ResolvedByName = resolverName;
        evt.ResolvedAt = DateTime.UtcNow;

        await _repository.UpdateAsync(evt);
        return true;
    }

    /// <summary>根据 ID 获取事件</summary>
    public async Task<AndonEvent?> GetByIdAsync(long id)
    {
        return await _repository.GetByIdAsync(id);
    }

    /// <summary>删除异常事件（软删除）</summary>
    public async Task<bool> DeleteEventAsync(long id)
    {
        var evt = await _repository.GetByIdAsync(id);
        if (evt == null) return false;

        await _repository.DeleteAsync(evt);
        return true;
    }

    /// <summary>获取未解决事件数量</summary>
    public async Task<int> GetActiveCountAsync()
    {
        return await _repository.CountAsync(e => e.ResolvedAt == null);
    }

    /// <summary>按类型统计未解决事件数量</summary>
    public async Task<Dictionary<Domain.Entities.AndonEventType, int>> GetActiveCountByTypeAsync()
    {
        var events = await _repository.FindAsync(e => e.ResolvedAt == null);
        return events
            .GroupBy(e => e.EventType)
            .ToDictionary(g => g.Key, g => g.Count());
    }
}
