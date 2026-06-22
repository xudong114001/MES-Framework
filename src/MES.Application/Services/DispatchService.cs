using MES.Application.Interfaces;
using MES.Domain.Entities;
using MES.Domain.Enums;
using MES.Domain.Repositories;

namespace MES.Application.Services;

/// <summary>
/// 派工服务：将已排产的工单/工序派工到具体工位
/// </summary>
public class DispatchService : IDispatchService
{
    private readonly IRepository<WorkOrder> _workOrderRepo;
    private readonly IRepository<WorkOrderStep> _stepRepo;
    private readonly IRepository<Workstation> _workstationRepo;
    private readonly IRepository<ProductionLine> _lineRepo;

    public DispatchService(
        IRepository<WorkOrder> workOrderRepo,
        IRepository<WorkOrderStep> stepRepo,
        IRepository<Workstation> workstationRepo,
        IRepository<ProductionLine> lineRepo)
    {
        _workOrderRepo = workOrderRepo;
        _stepRepo = stepRepo;
        _workstationRepo = workstationRepo;
        _lineRepo = lineRepo;
    }

    /// <summary>将工单的某工序派工到指定���位</summary>
    public async Task DispatchStepAsync(long workOrderStepId, long workstationId)
    {
        var step = await _stepRepo.GetByIdAsync(workOrderStepId)
            ?? throw new InvalidOperationException("工序不存在");

        var ws = await _workstationRepo.GetByIdAsync(workstationId)
            ?? throw new InvalidOperationException("工位不存在");

        step.AssignWorkstation(workstationId);
        step.UpdatedAt = DateTime.UtcNow;
        await _stepRepo.UpdateAsync(step);
    }

    /// <summary>取消某工序的派工（清除工位绑定）</summary>
    public async Task UndispatchStepAsync(long workOrderStepId)
    {
        var step = await _stepRepo.GetByIdAsync(workOrderStepId)
            ?? throw new InvalidOperationException("工序不存在");

        step.UnassignWorkstation();
        step.UpdatedAt = DateTime.UtcNow;
        await _stepRepo.UpdateAsync(step);
    }

    /// <summary>按产线查询今日已派工的任务（已经关联了工位的工序对应的工单）</summary>
    public async Task<IEnumerable<WorkOrder>> GetTodayDispatchedOrdersByLineAsync(long lineId)
    {
        var todayStart = DateTime.UtcNow.Date;
        var todayEnd = todayStart.AddDays(1);

        // 找出该产线下的所有工位 ID
        var workstations = await _workstationRepo.FindAsync(w => w.LineId == lineId);
        var wsIds = workstations.Select(w => w.Id).ToHashSet();
        if (!wsIds.Any()) return Enumerable.Empty<WorkOrder>();

        // 找出今日有派工记录的且工位属于该产线的工序
        var dispatchedSteps = await _stepRepo.FindAsync(s =>
            s.WorkstationId != null &&
            wsIds.Contains(s.WorkstationId!.Value));

        // 去重取工单 ID
        var woIds = dispatchedSteps.Select(s => s.WorkOrderId).Distinct().ToList();
        if (!woIds.Any()) return Enumerable.Empty<WorkOrder>();

        var result = new List<WorkOrder>();
        foreach (var woId in woIds)
        {
            var wo = await _workOrderRepo.GetByIdAsync(woId);
            if (wo != null)
            {
                var steps = await _stepRepo.FindAsync(s => s.WorkOrderId == woId);
                wo.Steps = steps.ToList();
                result.Add(wo);
            }
        }

        return result.OrderBy(wo => wo.Priority).ThenBy(wo => wo.PlanEndTime);
    }

    /// <summary>获取某产线下可选工位列表</summary>
    public async Task<IEnumerable<Workstation>> GetAvailableWorkstationsAsync(long lineId)
    {
        return await _workstationRepo.FindAsync(w => w.LineId == lineId && w.Status);
    }
}
