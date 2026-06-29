using MES.Application.Dtos;
using MES.Application.Interfaces;
using MES.Domain.Entities;
using MES.Domain.Enums;
using MES.Domain.Exceptions;
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

    private static WorkOrderDto MapWorkOrderToDto(WorkOrder entity) => new()
    {
        Id = entity.Id,
        OrderNo = entity.OrderNo,
        SourceType = entity.SourceType,
        SourceRef = entity.SourceRef,
        MaterialId = entity.MaterialId,
        RoutingId = entity.RoutingId,
        PlannedQty = entity.PlannedQty.Value,
        CompletedQty = entity.CompletedQty.Value,
        ScrapQty = entity.ScrapQty.Value,
        Status = entity.Status,
        PlanStartTime = entity.PlanStartTime,
        PlanEndTime = entity.PlanEndTime,
        ActualStartTime = entity.ActualStartTime,
        ActualEndTime = entity.ActualEndTime,
        Priority = entity.Priority,
        FactoryId = entity.FactoryId,
        WorkshopId = entity.WorkshopId,
        LineId = entity.LineId,
        Assignee = entity.Assignee,
        Remark = entity.Remark,
        ReworkFromId = entity.ReworkFromId,
        Steps = entity.Steps?.Select(MapStepToDto).ToList() ?? new(),
        CreatedAt = entity.CreatedAt,
        CreatedBy = entity.CreatedBy,
        UpdatedAt = entity.UpdatedAt,
        UpdatedBy = entity.UpdatedBy
    };

    private static WorkOrderStepDto MapStepToDto(WorkOrderStep entity) => new()
    {
        Id = entity.Id,
        WorkOrderId = entity.WorkOrderId,
        StepNo = entity.StepNo,
        StepName = entity.StepName,
        WorkstationId = entity.WorkstationId,
        PlannedQty = entity.PlannedQty,
        CompletedQty = entity.CompletedQty,
        ScrapQty = entity.ScrapQty,
        Status = entity.Status,
        PlanStartTime = entity.PlanStartTime,
        PlanEndTime = entity.PlanEndTime,
        CreatedAt = entity.CreatedAt,
        UpdatedAt = entity.UpdatedAt
    };

    private static WorkstationDto MapWorkstationToDto(Workstation entity) => new()
    {
        Id = entity.Id,
        LineId = entity.LineId,
        Code = entity.Code,
        Name = entity.Name,
        SeqNo = entity.SeqNo,
        Status = entity.Status,
        CreatedAt = entity.CreatedAt,
        CreatedBy = entity.CreatedBy,
        UpdatedAt = entity.UpdatedAt,
        UpdatedBy = entity.UpdatedBy
    };

    /// <summary>将工单的某工序派工到指定工位</summary>
    public async Task DispatchStepAsync(long workOrderStepId, long workstationId)
    {
        var step = await _stepRepo.GetByIdAsync(workOrderStepId)
            ?? throw new DomainException("工序不存在");

        var ws = await _workstationRepo.GetByIdAsync(workstationId)
            ?? throw new DomainException("工位不存在");

        step.AssignWorkstation(workstationId);
        await _stepRepo.UpdateAsync(step);
    }

    /// <summary>取消某工序的派工（清除工位绑定）</summary>
    public async Task UndispatchStepAsync(long workOrderStepId)
    {
        var step = await _stepRepo.GetByIdAsync(workOrderStepId)
            ?? throw new DomainException("工序不存在");

        step.UnassignWorkstation();
        await _stepRepo.UpdateAsync(step);
    }

    /// <summary>按产线查询今日已派工的任务（已经关联了工位的工序对应的工单）</summary>
    public async Task<IEnumerable<WorkOrderDto>> GetTodayDispatchedOrdersByLineAsync(long lineId)
    {
        var todayStart = DateTime.UtcNow.Date;
        var todayEnd = todayStart.AddDays(1);

        // 找出该产线下的所有工位 ID
        var workstations = await _workstationRepo.FindAsync(w => w.LineId == lineId);
        var wsIds = workstations.Select(w => w.Id).ToHashSet();
        if (!wsIds.Any()) return Enumerable.Empty<WorkOrderDto>();

        // 找出今日有派工记录的且工位属于该产线的工序
        var dispatchedSteps = await _stepRepo.FindAsync(s =>
            s.WorkstationId != null &&
            wsIds.Contains(s.WorkstationId!.Value));

        // 去重取工单 ID
        var woIds = dispatchedSteps.Select(s => s.WorkOrderId).Distinct().ToList();
        if (!woIds.Any()) return Enumerable.Empty<WorkOrderDto>();

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

        return result
            .OrderBy(wo => wo.Priority)
            .ThenBy(wo => wo.PlanEndTime)
            .Select(MapWorkOrderToDto);
    }

    /// <summary>获取某产线下可选工位列表</summary>
    public async Task<IEnumerable<WorkstationDto>> GetAvailableWorkstationsAsync(long lineId)
    {
        var workstations = await _workstationRepo.FindAsync(w => w.LineId == lineId && w.Status);
        return workstations.Select(MapWorkstationToDto);
    }
}
