using MES.Application.Interfaces;
using MES.Domain.Entities;
using MES.Domain.Enums;
using MES.Domain.Repositories;

namespace MES.Application.Services;

/// <summary>
/// 排产引擎服务
/// </summary>
public class SchedulingService : ISchedulingService
{
    private readonly IRepository<WorkOrder> _workOrderRepo;
    private readonly IRepository<ProductionLine> _lineRepo;
    private readonly IRepository<WorkOrderStep> _stepRepo2;

    public SchedulingService(
        IRepository<WorkOrder> workOrderRepo,
        IRepository<ProductionLine> lineRepo,
        IRepository<WorkOrderStep> stepRepo2)
    {
        _workOrderRepo = workOrderRepo;
        _lineRepo = lineRepo;
        _stepRepo2 = stepRepo2;
    }

    /// <summary>获取所有已下达(RELEASED)且未排产的工单</summary>
    public async Task<IEnumerable<WorkOrder>> GetUnscheduledOrdersAsync()
    {
        return await _workOrderRepo.FindAsync(wo =>
            wo.Status == WorkOrderStatus.RELEASED && wo.LineId == null);
    }

    /// <summary>排产：将单个工单分配到指定产线，状态 RELEASED → SCHEDULED</summary>
    public async Task ScheduleOrderAsync(long workOrderId, long lineId)
    {
        var wo = await _workOrderRepo.GetByIdAsync(workOrderId)
            ?? throw new InvalidOperationException("工单不存在");

        if (wo.Status != WorkOrderStatus.RELEASED)
            throw new InvalidOperationException($"工单状态 {wo.Status} 不允许排产，仅 RELEASED 可排产");

        var line = await _lineRepo.GetByIdAsync(lineId)
            ?? throw new InvalidOperationException("产线不存在");

        wo.Schedule(lineId);
        await _workOrderRepo.UpdateAsync(wo);

        // 如果有工序步骤，设置计划时间（工位由派工环节决定）
        var steps = await _stepRepo2.FindAsync(s => s.WorkOrderId == workOrderId);
        if (steps.Any())
        {
            var orderedSteps = steps.OrderBy(s => s.StepNo).ToList();
            foreach (var step in orderedSteps)
            {
                step.UpdatePlannedTimes(wo.PlanStartTime, wo.PlanEndTime);
                await _stepRepo2.UpdateAsync(step);
            }
        }
    }

    /// <summary>批量排产：将多个工单按优先级+交期排序后分配到指定产线</summary>
    public async Task ScheduleOrdersAsync(IEnumerable<long> workOrderIds, long lineId)
    {
        var sorted = workOrderIds
            .Select(async id => await _workOrderRepo.GetByIdAsync(id))
            .Select(t => t.Result)
            .Where(wo => wo != null && wo.Status == WorkOrderStatus.RELEASED)
            .OrderByDescending(wo => wo!.Priority)
            .ThenBy(wo => wo!.PlanEndTime)
            .ToList();

        foreach (var wo in sorted)
        {
            if (wo != null)
                await ScheduleOrderAsync(wo.Id, lineId);
        }
    }

    /// <summary>自动排产：为所有 RELEASED 且未分配产线的工单自动选择产线</summary>
    public async Task AutoScheduleAsync()
    {
        var unscheduled = (await _workOrderRepo.FindAsync(wo =>
            wo.Status == WorkOrderStatus.RELEASED && wo.LineId == null))
            .OrderByDescending(wo => wo.Priority)
            .ThenBy(wo => wo.PlanEndTime)
            .ToList();

        if (!unscheduled.Any()) return;

        // 简单策略：按产线当前负载分配（轮询所有可用产线）
        var lines = await _lineRepo.FindAsync(l => l.Status);
        if (!lines.Any()) throw new InvalidOperationException("没有可用的产线");

        var lineList = lines.ToList();
        for (int i = 0; i < unscheduled.Count; i++)
        {
            var line = lineList[i % lineList.Count];
            await ScheduleOrderAsync(unscheduled[i].Id, line.Id);
        }
    }

    /// <summary>调整排产顺序（交换两条已排产的同线工单的 LineId，用于拖拽调整）</summary>
    public async Task SwapSchedulingOrderAsync(long orderId1, long orderId2)
    {
        var wo1 = await _workOrderRepo.GetByIdAsync(orderId1)
            ?? throw new InvalidOperationException("工单1不存在");
        var wo2 = await _workOrderRepo.GetByIdAsync(orderId2)
            ?? throw new InvalidOperationException("工单2不存在");

        if (wo1.LineId == null || wo2.LineId == null)
            throw new InvalidOperationException("两条工单必须都已排产");

        // 简单的交换：更新更新时间（作为排序依据），或者后续支持 SortOrder 字段
        // 此处通过交换 UpdatedAt 来实现顺序调整
        var tempTime = wo1.UpdatedAt;
        wo1.UpdatedAt = wo2.UpdatedAt;
        wo2.UpdatedAt = tempTime;
        await _workOrderRepo.UpdateAsync(wo1);
        await _workOrderRepo.UpdateAsync(wo2);
    }

    /// <summary>获取指定产线的所有已排产工单（含工序）</summary>
    public async Task<IEnumerable<WorkOrder>> GetScheduledOrdersByLineAsync(long lineId)
    {
        var orders = (await _workOrderRepo.FindAsync(wo =>
            wo.LineId == lineId &&
            (wo.Status == WorkOrderStatus.SCHEDULED || wo.Status == WorkOrderStatus.IN_PROGRESS)))
            .OrderByDescending(wo => wo.Priority)
            .ThenBy(wo => wo.PlanEndTime);

        // 加载工序
        foreach (var wo in orders)
        {
            var steps = await _stepRepo2.FindAsync(s => s.WorkOrderId == wo.Id);
            wo.Steps = steps.ToList();
        }

        return orders;
    }

    /// <summary>取消排产：SCHEDULED → RELEASED，清除 LineId</summary>
    public async Task UnscheduleOrderAsync(long workOrderId)
    {
        var wo = await _workOrderRepo.GetByIdAsync(workOrderId)
            ?? throw new InvalidOperationException("工单不存在");

        if (wo.Status != WorkOrderStatus.SCHEDULED)
            throw new InvalidOperationException($"工单状态 {wo.Status} 不允许取消排产，仅 SCHEDULED 可操作");

        wo.Unschedule();
        await _workOrderRepo.UpdateAsync(wo);
    }
}
