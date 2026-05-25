using Microsoft.Extensions.Logging;
using MES.Application.Interfaces;
using MES.Domain.Entities;
using MES.Domain.Enums;
using MES.Infrastructure.Repositories;

namespace MES.Application.Services;

/// <summary>
/// 工单（P1）服务
/// </summary>
public class WorkOrderService : IWorkOrderService
{
    private readonly IRepository<WorkOrder> _workOrderRepo;
    private readonly IRepository<WorkOrderStep> _stepRepo;
    private readonly IRepository<Material> _materialRepo;
    private readonly IRepository<Routing> _routingRepo;
    private readonly IRepository<Bom> _bomRepo;
    private readonly ILogger<WorkOrderService> _logger;

    public WorkOrderService(
        IRepository<WorkOrder> workOrderRepo,
        IRepository<WorkOrderStep> stepRepo,
        IRepository<Material> materialRepo,
        IRepository<Routing> routingRepo,
        IRepository<Bom> bomRepo,
        ILogger<WorkOrderService> logger)
    {
        _workOrderRepo = workOrderRepo;
        _stepRepo = stepRepo;
        _materialRepo = materialRepo;
        _routingRepo = routingRepo;
        _bomRepo = bomRepo;
        _logger = logger;
    }

    /// <summary>
    /// 创建工单，默认状态 PENDING，校验物料是否存在
    /// </summary>
    public async Task<WorkOrder> CreateWorkOrderAsync(WorkOrder workOrder)
    {
        // 验证物料存在
        var material = await _materialRepo.GetByIdAsync(workOrder.MaterialId);
        if (material == null)
            throw new InvalidOperationException($"物料不存在 (MaterialId={workOrder.MaterialId})");

        // BOM 库存校验
        var bomComponents = await _bomRepo.FindAsync(b => b.ProductId == workOrder.MaterialId && b.Status);
        if (bomComponents.Any())
        {
            foreach (var bomItem in bomComponents)
            {
                var component = await _materialRepo.GetByIdAsync(bomItem.MaterialId);
                if (component == null) continue;

                var requiredQty = bomItem.Quantity * workOrder.PlannedQty;
                if (component.StockQty < requiredQty)
                {
                    throw new InvalidOperationException(
                        $"物料 {component.Name} 库存不足（需要 {requiredQty}，可用 {component.StockQty}）");
                }
            }

            // BOM 配置校验：有 BOM 但 routingId 为空，只给警告（工艺路线不是必选的）
            if (!workOrder.RoutingId.HasValue)
            {
                _logger.LogWarning(
                    "物料 {MaterialName}(ID={MaterialId}) 已配置 BOM，但创建工单时未选择工艺路线",
                    material.Name, workOrder.MaterialId);
            }
        }
        else
        {
            // 物料没有 BOM 配置，记录提示日志，跳过库存校验
            _logger.LogInformation(
                "物料 {MaterialName}(ID={MaterialId}) 未配置 BOM，跳过库存校验",
                material.Name, workOrder.MaterialId);
        }

        workOrder.Status = WorkOrderStatus.PENDING;
        workOrder.CompletedQty = 0;
        workOrder.ScrapQty = 0;

        var created = await _workOrderRepo.AddAsync(workOrder);

        // 如果提供了 RoutingId，自动生成工序任务（状态 PENDING）
        if (workOrder.RoutingId.HasValue)
        {
            var routing = await _routingRepo.GetByIdAsync(workOrder.RoutingId.Value);
            if (routing != null)
            {
                // 按 RoutingStep 顺序生成 WorkOrderStep
                foreach (var step in routing.Steps.OrderBy(s => s.StepNo))
                {
                    var woStep = new WorkOrderStep
                    {
                        WorkOrderId = created.Id,
                        StepNo = step.StepNo,
                        StepName = step.StepName,
                        WorkstationId = null,  // 由派工决定
                        PlannedQty = workOrder.PlannedQty,
                        CompletedQty = 0,
                        ScrapQty = 0,
                        Status = WorkOrderStatus.PENDING,
                        PlanStartTime = workOrder.PlanStartTime,
                        PlanEndTime = workOrder.PlanEndTime
                    };
                    await _stepRepo.AddAsync(woStep);
                }
            }
        }

        return created;
    }

    /// <summary>
    /// 下达工单：PENDING → RELEASED
    /// 如果有 RoutingId 则同时生成工序任务（WorkOrderStep）
    /// </summary>
    public async Task ReleaseWorkOrderAsync(long workOrderId)
    {
        var wo = await _workOrderRepo.GetByIdAsync(workOrderId);
        if (wo == null) throw new InvalidOperationException("工单不存在");

        if (wo.Status != WorkOrderStatus.PENDING)
            throw new InvalidOperationException($"工单状态 {wo.Status} 不允许下达，仅 PENDING 可下达");

        wo.Status = WorkOrderStatus.RELEASED;
        await _workOrderRepo.UpdateAsync(wo);

        // 如果有 RoutingId 且尚未生成工序，则生成
        if (wo.RoutingId.HasValue)
        {
            var existingSteps = await _stepRepo.FindAsync(s => s.WorkOrderId == workOrderId);
            if (!existingSteps.Any())
            {
                var routing = await _routingRepo.GetByIdAsync(wo.RoutingId.Value);
                if (routing != null)
                {
                    foreach (var step in routing.Steps.OrderBy(s => s.StepNo))
                    {
                        var woStep = new WorkOrderStep
                        {
                            WorkOrderId = workOrderId,
                            StepNo = step.StepNo,
                            StepName = step.StepName,
                            WorkstationId = null,
                            PlannedQty = wo.PlannedQty,
                            CompletedQty = 0,
                            ScrapQty = 0,
                            Status = WorkOrderStatus.PENDING,
                            PlanStartTime = wo.PlanStartTime,
                            PlanEndTime = wo.PlanEndTime
                        };
                        await _stepRepo.AddAsync(woStep);
                    }
                }
            }
        }
    }

    /// <summary>
    /// 暂停工单：RELEASED / IN_PROGRESS → ON_HOLD
    /// </summary>
    public async Task HoldWorkOrderAsync(long workOrderId)
    {
        var wo = await _workOrderRepo.GetByIdAsync(workOrderId);
        if (wo == null) throw new InvalidOperationException("工单不存在");

        if (wo.Status != WorkOrderStatus.RELEASED && wo.Status != WorkOrderStatus.IN_PROGRESS)
            throw new InvalidOperationException($"工单状态 {wo.Status} 不允许暂停，仅 RELEASED/IN_PROGRESS 可暂停");

        wo.Status = WorkOrderStatus.ON_HOLD;
        await _workOrderRepo.UpdateAsync(wo);
    }

    /// <summary>
    /// 恢复工单：ON_HOLD → IN_PROGRESS
    /// </summary>
    public async Task ResumeWorkOrderAsync(long workOrderId)
    {
        var wo = await _workOrderRepo.GetByIdAsync(workOrderId);
        if (wo == null) throw new InvalidOperationException("工单不存在");

        if (wo.Status != WorkOrderStatus.ON_HOLD)
            throw new InvalidOperationException($"工单状态 {wo.Status} 不允许恢复，仅 ON_HOLD 可恢复");

        wo.Status = WorkOrderStatus.IN_PROGRESS;
        await _workOrderRepo.UpdateAsync(wo);
    }

    /// <summary>
    /// 取消工单：PENDING / RELEASED / ON_HOLD → CANCELLED
    /// </summary>
    public async Task CancelWorkOrderAsync(long workOrderId)
    {
        var wo = await _workOrderRepo.GetByIdAsync(workOrderId);
        if (wo == null) throw new InvalidOperationException("工单不存在");

        if (wo.Status != WorkOrderStatus.PENDING
            && wo.Status != WorkOrderStatus.RELEASED
            && wo.Status != WorkOrderStatus.ON_HOLD)
            throw new InvalidOperationException($"工单状态 {wo.Status} 不允许取消，仅 PENDING/RELEASED/ON_HOLD 可取消");

        wo.Status = WorkOrderStatus.CANCELLED;
        await _workOrderRepo.UpdateAsync(wo);
    }

    /// <summary>
    /// 关闭工单：COMPLETED → CLOSED
    /// </summary>
    public async Task CloseWorkOrderAsync(long workOrderId)
    {
        var wo = await _workOrderRepo.GetByIdAsync(workOrderId);
        if (wo == null) throw new InvalidOperationException("工单不存在");

        if (wo.Status != WorkOrderStatus.COMPLETED)
            throw new InvalidOperationException($"工单状态 {wo.Status} 不允许关闭，仅 COMPLETED 可关闭");

        wo.Status = WorkOrderStatus.CLOSED;
        wo.ActualEndTime = DateTime.UtcNow;
        await _workOrderRepo.UpdateAsync(wo);
    }

    /// <summary>
    /// 拆分工单：扣减原单数量，创建子单
    /// 原单必须是 PENDING / RELEASED 状态
    /// </summary>
    public async Task<WorkOrder> SplitWorkOrderAsync(long workOrderId, decimal splitQty)
    {
        var wo = await _workOrderRepo.GetByIdAsync(workOrderId);
        if (wo == null) throw new InvalidOperationException("工单不存在");

        if (wo.Status != WorkOrderStatus.PENDING && wo.Status != WorkOrderStatus.RELEASED)
            throw new InvalidOperationException($"工单状态 {wo.Status} 不允许拆分，仅 PENDING/RELEASED 可拆分");

        if (splitQty <= 0)
            throw new InvalidOperationException("拆分数量必须大于 0");

        if (splitQty >= wo.PlannedQty)
            throw new InvalidOperationException("拆分数量必须小于原单计划数量");

        // 扣减原单数量
        wo.PlannedQty -= splitQty;
        await _workOrderRepo.UpdateAsync(wo);

        // 创建子单
        var child = new WorkOrder
        {
            OrderNo = $"{wo.OrderNo}-SUB",
            SourceType = wo.SourceType,
            SourceRef = $"SplitFrom:{workOrderId}",
            MaterialId = wo.MaterialId,
            RoutingId = wo.RoutingId,
            PlannedQty = splitQty,
            CompletedQty = 0,
            ScrapQty = 0,
            Status = WorkOrderStatus.PENDING,
            PlanStartTime = wo.PlanStartTime,
            PlanEndTime = wo.PlanEndTime,
            Priority = wo.Priority,
            FactoryId = wo.FactoryId,
            WorkshopId = wo.WorkshopId,
            LineId = wo.LineId,
            Assignee = wo.Assignee,
            Remark = $"工单拆分自 {wo.OrderNo}(ID={workOrderId})"
        };

        var created = await _workOrderRepo.AddAsync(child);

        // 子单也生成工序步骤（如果有 RoutingId）
        if (wo.RoutingId.HasValue)
        {
            var routing = await _routingRepo.GetByIdAsync(wo.RoutingId.Value);
            if (routing != null)
            {
                foreach (var step in routing.Steps.OrderBy(s => s.StepNo))
                {
                    var woStep = new WorkOrderStep
                    {
                        WorkOrderId = created.Id,
                        StepNo = step.StepNo,
                        StepName = step.StepName,
                        WorkstationId = null,
                        PlannedQty = splitQty,
                        CompletedQty = 0,
                        ScrapQty = 0,
                        Status = WorkOrderStatus.PENDING,
                        PlanStartTime = wo.PlanStartTime,
                        PlanEndTime = wo.PlanEndTime
                    };
                    await _stepRepo.AddAsync(woStep);
                }
            }
        }

        return created;
    }

    /// <summary>
    /// 返工：从 COMPLETED/IN_PROGRESS 工单创建返工子工单
    /// </summary>
    public async Task<WorkOrder> ReworkWorkOrderAsync(long workOrderId, decimal reworkQty, string? remark)
    {
        var wo = await _workOrderRepo.GetByIdAsync(workOrderId);
        if (wo == null) throw new InvalidOperationException("工单不存在");

        if (wo.Status != WorkOrderStatus.COMPLETED && wo.Status != WorkOrderStatus.IN_PROGRESS)
            throw new InvalidOperationException($"工单状态 {wo.Status} 不允许返工，仅 COMPLETED/IN_PROGRESS 可返工");

        if (reworkQty <= 0)
            throw new InvalidOperationException("返工数量必须大于 0");

        if (reworkQty > wo.CompletedQty)
            throw new InvalidOperationException($"返工数量({reworkQty})超过已完成数量({wo.CompletedQty})");

        // 扣减原工单已完成数量
        wo.CompletedQty -= reworkQty;
        await _workOrderRepo.UpdateAsync(wo);

        // 创建返工子工单
        var child = new WorkOrder
        {
            OrderNo = $"{wo.OrderNo}-RWK",
            SourceType = wo.SourceType,
            SourceRef = $"ReworkFrom:{workOrderId}",
            MaterialId = wo.MaterialId,
            RoutingId = wo.RoutingId,
            PlannedQty = reworkQty,
            CompletedQty = 0,
            ScrapQty = 0,
            Status = WorkOrderStatus.PENDING,
            PlanStartTime = wo.PlanStartTime,
            PlanEndTime = wo.PlanEndTime,
            Priority = wo.Priority,
            FactoryId = wo.FactoryId,
            WorkshopId = wo.WorkshopId,
            LineId = wo.LineId,
            Assignee = wo.Assignee,
            Remark = remark ?? $"返工自 {wo.OrderNo}(ID={workOrderId})",
            ReworkFromId = workOrderId
        };

        var created = await _workOrderRepo.AddAsync(child);

        // 子单也生成工序步骤
        if (wo.RoutingId.HasValue)
        {
            var routing = await _routingRepo.GetByIdAsync(wo.RoutingId.Value);
            if (routing != null)
            {
                foreach (var step in routing.Steps.OrderBy(s => s.StepNo))
                {
                    var woStep = new WorkOrderStep
                    {
                        WorkOrderId = created.Id,
                        StepNo = step.StepNo,
                        StepName = step.StepName,
                        WorkstationId = null,
                        PlannedQty = reworkQty,
                        CompletedQty = 0,
                        ScrapQty = 0,
                        Status = WorkOrderStatus.PENDING,
                        PlanStartTime = wo.PlanStartTime,
                        PlanEndTime = wo.PlanEndTime
                    };
                    await _stepRepo.AddAsync(woStep);
                }
            }
        }

        return created;
    }

    /// <summary>
    /// 独立报废操作
    /// </summary>
    public async Task ScrapWorkOrderAsync(long workOrderId, decimal scrapQty, string? remark)
    {
        var wo = await _workOrderRepo.GetByIdAsync(workOrderId);
        if (wo == null) throw new InvalidOperationException("工单不存在");

        if (wo.Status != WorkOrderStatus.IN_PROGRESS && wo.Status != WorkOrderStatus.RELEASED)
            throw new InvalidOperationException($"工单状态 {wo.Status} 不允许报废，仅 IN_PROGRESS/RELEASED 可报废");

        if (scrapQty <= 0)
            throw new InvalidOperationException("报废数量必须大于 0");

        var remaining = wo.PlannedQty - wo.CompletedQty - wo.ScrapQty;
        if (scrapQty > remaining)
            throw new InvalidOperationException($"报废数量({scrapQty})超过剩余可操作数量({remaining})");

        wo.ScrapQty += scrapQty;
        wo.Remark = remark ?? wo.Remark;

        // 如果全部报废，标记取消
        if (wo.CompletedQty + wo.ScrapQty >= wo.PlannedQty)
            wo.Status = WorkOrderStatus.CANCELLED;

        await _workOrderRepo.UpdateAsync(wo);
    }
}
