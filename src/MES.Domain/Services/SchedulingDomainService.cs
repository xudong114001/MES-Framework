using MES.Domain.Entities;
using MES.Domain.Enums;

namespace MES.Domain.Services;

/// <summary>
/// 排程领域服务 - 封装纯领域的排程优先级算法
/// </summary>
public class SchedulingDomainService
{
    /// <summary>
    /// 按优先级 + 交期排序 - 纯领域算法，无 I/O
    /// 排序规则：优先级降序 → 计划结束时间升序 → 创建时间升序
    /// </summary>
    /// <param name="orders">待排序的工单集合</param>
    /// <returns>排序后的只读列表</returns>
    public IReadOnlyList<WorkOrder> PrioritizeOrders(IEnumerable<WorkOrder> orders)
    {
        return orders
            .OrderByDescending(o => o.Priority)
            .ThenBy(o => o.PlanEndTime)
            .ThenBy(o => o.CreatedAt)
            .ToList()
            .AsReadOnly();
    }
}
