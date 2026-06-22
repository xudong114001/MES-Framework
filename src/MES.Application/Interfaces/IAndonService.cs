using MES.Domain.Entities;

namespace MES.Application.Interfaces;

public interface IAndonService
{
    /// <summary>获取所有活跃异常事件</summary>
    Task<IEnumerable<AndonEvent>> GetActiveEventsAsync();
    
    /// <summary>获取所有异常事件</summary>
    Task<IEnumerable<AndonEvent>> GetAllEventsAsync();
    
    /// <summary>获取异常事件（分页）</summary>
    Task<(IEnumerable<AndonEvent> Items, int Total)> GetEventsAsync(int page = 1, int pageSize = 20, bool? isResolved = null, AndonEventType? eventType = null);
    
    /// <summary>触发异常事件</summary>
    Task<AndonEvent> TriggerEventAsync(AndonEventType eventType, AndonEventLevel level, string title, string? description = null, long? workstationId = null, string? workstationName = null, long? workOrderId = null, string? workOrderNo = null, long? triggeredById = null, string? triggeredByName = null);
    
    /// <summary>解决异常事件</summary>
    Task<bool> ResolveEventAsync(long eventId, long resolverId, string resolverName);
    
    /// <summary>根据ID获取事件</summary>
    Task<AndonEvent?> GetByIdAsync(long id);
    
    /// <summary>删除异常事件</summary>
    Task<bool> DeleteEventAsync(long id);
    
    /// <summary>获取未解决事件数量</summary>
    Task<int> GetActiveCountAsync();
    
    /// <summary>按类型统计未解决事件</summary>
    Task<Dictionary<AndonEventType, int>> GetActiveCountByTypeAsync();
}
