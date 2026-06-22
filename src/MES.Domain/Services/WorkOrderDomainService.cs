using MES.Domain.Entities;
using MES.Domain.Enums;
using MES.Domain.Exceptions;

namespace MES.Domain.Services;

/// <summary>
/// 工单领域服务 - 封装跨聚合或纯内存的状态转换校验逻辑
/// </summary>
public class WorkOrderDomainService
{
    /// <summary>
    /// 工单状态合法转换规则表
    /// 键：当前状态，值：允许转换到的目标状态集合
    /// </summary>
    private static readonly Dictionary<WorkOrderStatus, WorkOrderStatus[]> ValidTransitions = new()
    {
        [WorkOrderStatus.PENDING] = [WorkOrderStatus.RELEASED, WorkOrderStatus.CANCELLED],
        [WorkOrderStatus.RELEASED] = [WorkOrderStatus.SCHEDULED, WorkOrderStatus.IN_PROGRESS, WorkOrderStatus.ON_HOLD, WorkOrderStatus.CANCELLED],
        [WorkOrderStatus.SCHEDULED] = [WorkOrderStatus.IN_PROGRESS, WorkOrderStatus.RELEASED],
        [WorkOrderStatus.IN_PROGRESS] = [WorkOrderStatus.COMPLETED, WorkOrderStatus.ON_HOLD],
        [WorkOrderStatus.COMPLETED] = [WorkOrderStatus.CLOSED],
        [WorkOrderStatus.ON_HOLD] = [WorkOrderStatus.IN_PROGRESS, WorkOrderStatus.CANCELLED],
        [WorkOrderStatus.CLOSED] = [],
        [WorkOrderStatus.CANCELLED] = []
    };

    /// <summary>
    /// 校验工单状态转换是否合法（纯内存校验）
    /// </summary>
    /// <param name="current">当前状态</param>
    /// <param name="target">目标状态</param>
    /// <exception cref="DomainException">当状态转换不合法时抛出</exception>
    public void ValidateStatusTransition(WorkOrderStatus current, WorkOrderStatus target)
    {
        if (current == target)
            throw new DomainException($"工单已处于 {current} 状态，无需转换");

        if (!ValidTransitions.TryGetValue(current, out var allowed) || !allowed.Contains(target))
            throw new DomainException($"不允许从 {current} 转换到 {target}");
    }

    /// <summary>
    /// 获取指定状态允许转换到的所有目标状态
    /// </summary>
    public IReadOnlyList<WorkOrderStatus> GetAllowedTransitions(WorkOrderStatus current)
    {
        if (ValidTransitions.TryGetValue(current, out var allowed))
            return allowed.ToList().AsReadOnly();

        return Array.Empty<WorkOrderStatus>().AsReadOnly();
    }
}
