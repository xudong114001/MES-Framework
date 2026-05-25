using MES.Domain.Entities;

namespace MES.Application.Interfaces;

/// <summary>
/// 排产引擎服务接口
/// </summary>
public interface ISchedulingService
{
    /// <summary>获取所有已下达且未排产的工单</summary>
    Task<IEnumerable<WorkOrder>> GetUnscheduledOrdersAsync();

    /// <summary>排产：将工单分配到指定产线，状态从 RELEASED → SCHEDULED</summary>
    Task ScheduleOrderAsync(long workOrderId, long lineId);

    /// <summary>批量排产：按优先级+交期排序后分配到指定产线</summary>
    Task ScheduleOrdersAsync(IEnumerable<long> workOrderIds, long lineId);

    /// <summary>自动排产：将所有 RELEASED 工单按优先级高→低、交期近→远排序后分配到合适的产线</summary>
    Task AutoScheduleAsync();

    /// <summary>调整排产顺序（交换两条已排产到同一条线的工单的线内排产序列）</summary>
    Task SwapSchedulingOrderAsync(long orderId1, long orderId2);

    /// <summary>获取指定产线的所有已排产工单（含工序）</summary>
    Task<IEnumerable<WorkOrder>> GetScheduledOrdersByLineAsync(long lineIdcounter);

    /// <summary>取消排产：工单从 SCHEDULED 回到 RELEASED</summary>
    Task UnscheduleOrderAsync(long workOrderId);
}
