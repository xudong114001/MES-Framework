using Microsoft.Extensions.Logging;
using MES.Application.Dtos;
using MES.Application.Interfaces;
using MES.Application.Integration.Events;
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
    private readonly IEventBus? _eventBus;
    private readonly InMemoryEventLogService? _eventLog;

    public WorkOrderService(
        IRepository<WorkOrder> workOrderRepo,
        IRepository<WorkOrderStep> stepRepo,
        IRepository<Material> materialRepo,
        IRepository<Routing> routingRepo,
        IRepository<Bom> bomRepo,
        ILogger<WorkOrderService> logger,
        IEventBus? eventBus = null,
        InMemoryEventLogService? eventLog = null)
    {
        _workOrderRepo = workOrderRepo;
        _stepRepo = stepRepo;
        _materialRepo = materialRepo;
        _routingRepo = routingRepo;
        _bomRepo = bomRepo;
        _logger = logger;
        _eventBus = eventBus;
        _eventLog = eventLog;
    }

    /// <summary>
    /// 将 WorkOrder 实体映射为 DTO
    /// </summary>
    private static WorkOrderDto MapToDto(WorkOrder entity)
    {
        return new WorkOrderDto
        {
            Id = entity.Id,
            OrderNo = entity.OrderNo,
            SourceType = entity.SourceType,
            SourceRef = entity.SourceRef,
            MaterialId = entity.MaterialId,
            RoutingId = entity.RoutingId,
            PlannedQty = entity.PlannedQty,
            CompletedQty = entity.CompletedQty,
            ScrapQty = entity.ScrapQty,
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
            CreatedAt = entity.CreatedAt,
            CreatedBy = entity.CreatedBy,
            UpdatedAt = entity.UpdatedAt,
            UpdatedBy = entity.UpdatedBy
        };
    }

    /// <summary>
    /// 获取所有工单
    /// </summary>
    public async Task<IEnumerable<WorkOrderDto>> GetAllAsync()
    {
        var entities = await _workOrderRepo.GetAllAsync();
        return entities.Select(MapToDto);
    }

    /// <summary>
    /// 根据ID获取工单
    /// </summary>
    public async Task<WorkOrderDto?> GetByIdAsync(long id)
    {
        var entity = await _workOrderRepo.GetByIdAsync(id);
        return entity == null ? null : MapToDto(entity);
    }

    /// <summary>
    /// 更新工单
    /// </summary>
    public async Task UpdateWorkOrderAsync(WorkOrder workOrder)
    {
        var existing = await _workOrderRepo.GetByIdAsync(workOrder.Id);
        if (existing == null)
            throw new InvalidOperationException("工单不存在");

        // 使用实体方法更新计划时间
        existing.UpdatePlannedTimes(workOrder.PlanStartTime, workOrder.PlanEndTime);

        // 更新其他可更新字段（通过私有 setter）
        // 注意：这里只能更新允许外部修改的字段，状态变更必须通过业务方法

        await _workOrderRepo.UpdateAsync(existing);
    }

    /// <summary>
    /// 删除工单
    /// </summary>
    public async Task DeleteWorkOrderAsync(long id)
    {
        var entity = await _workOrderRepo.GetByIdAsync(id);
        if (entity == null)
            throw new InvalidOperationException("工单不存在");

        await _workOrderRepo.DeleteAsync(entity);
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

        // 使用工厂方法创建工单
        var created = WorkOrder.Create(
            orderNo: workOrder.OrderNo,
            sourceType: workOrder.SourceType,
            materialId: workOrder.MaterialId,
            plannedQty: workOrder.PlannedQty,
            priority: workOrder.Priority,
            routingId: workOrder.RoutingId,
            sourceRef: workOrder.SourceRef,
            planStartTime: workOrder.PlanStartTime,
            planEndTime: workOrder.PlanEndTime,
            factoryId: workOrder.FactoryId,
            workshopId: workOrder.WorkshopId,
            lineId: workOrder.LineId,
            assignee: workOrder.Assignee,
            remark: workOrder.Remark
        );

        await _workOrderRepo.AddAsync(created);

        // 发布工单创建事件（失败不影响主事务）
        try
        {
            if (_eventBus != null)
            {
                var evt = new WorkOrderCreatedEvent
                {
                    WorkOrderId = created.Id,
                    OrderNo = created.OrderNo,
                    MaterialId = created.MaterialId,
                    PlannedQty = created.PlannedQty,
                    SourceRef = created.SourceRef
                };
                await _eventBus.Publish(evt);
                _eventLog?.Log(evt, "Published");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish WorkOrderCreatedEvent");
            _eventLog?.Log(new WorkOrderCreatedEvent
            {
                WorkOrderId = created.Id,
                OrderNo = created.OrderNo,
                MaterialId = created.MaterialId,
                PlannedQty = created.PlannedQty
            }, "Failed", ex.Message);
        }

        // 如果提供了 RoutingId，自动生成工序任务（状态 PENDING）
        if (workOrder.RoutingId.HasValue)
        {
            var routing = await _routingRepo.GetByIdAsync(workOrder.RoutingId.Value);
            if (routing != null)
            {
                // 按 RoutingStep 顺序生成 WorkOrderStep
                foreach (var step in routing.Steps.OrderBy(s => s.StepNo))
                {
                    var woStep = WorkOrderStep.Create(
                        workOrderId: created.Id,
                        stepNo: step.StepNo,
                        stepName: step.StepName,
                        plannedQty: workOrder.PlannedQty,
                        planStartTime: workOrder.PlanStartTime,
                        planEndTime: workOrder.PlanEndTime
                    );
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

        var oldStatus = wo.Status;

        // 使用实体方法下达工单
        wo.Release();
        await _workOrderRepo.UpdateAsync(wo);

        // 发布工单状态变更事件（失败不影响主事务）
        try
        {
            if (_eventBus != null)
            {
                var evt = new WorkOrderStatusChangedEvent
                {
                    WorkOrderId = wo.Id,
                    OrderNo = wo.OrderNo,
                    OldStatus = oldStatus,
                    NewStatus = WorkOrderStatus.RELEASED,
                    ChangedAt = DateTime.UtcNow
                };
                await _eventBus.Publish(evt);
                _eventLog?.Log(evt, "Published");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish WorkOrderStatusChangedEvent");
            _eventLog?.Log(new WorkOrderStatusChangedEvent
            {
                WorkOrderId = wo.Id,
                OrderNo = wo.OrderNo,
                OldStatus = oldStatus,
                NewStatus = WorkOrderStatus.RELEASED
            }, "Failed", ex.Message);
        }

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
                        var woStep = WorkOrderStep.Create(
                            workOrderId: workOrderId,
                            stepNo: step.StepNo,
                            stepName: step.StepName,
                            plannedQty: wo.PlannedQty,
                            planStartTime: wo.PlanStartTime,
                            planEndTime: wo.PlanEndTime
                        );
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

        // 使用实体方法暂停工单
        wo.Hold();
        await _workOrderRepo.UpdateAsync(wo);
    }

    /// <summary>
    /// 恢复工单：ON_HOLD → IN_PROGRESS
    /// </summary>
    public async Task ResumeWorkOrderAsync(long workOrderId)
    {
        var wo = await _workOrderRepo.GetByIdAsync(workOrderId);
        if (wo == null) throw new InvalidOperationException("工单不存在");

        // 使用实体方法恢复工单
        wo.Resume();
        await _workOrderRepo.UpdateAsync(wo);
    }

    /// <summary>
    /// 取消工单：PENDING / RELEASED / ON_HOLD → CANCELLED
    /// </summary>
    public async Task CancelWorkOrderAsync(long workOrderId)
    {
        var wo = await _workOrderRepo.GetByIdAsync(workOrderId);
        if (wo == null) throw new InvalidOperationException("工单不存在");

        // 使用实体方法取消工单
        wo.Cancel();
        await _workOrderRepo.UpdateAsync(wo);
    }

    /// <summary>
    /// 关闭工单：COMPLETED → CLOSED
    /// </summary>
    public async Task CloseWorkOrderAsync(long workOrderId)
    {
        var wo = await _workOrderRepo.GetByIdAsync(workOrderId);
        if (wo == null) throw new InvalidOperationException("工单不存在");

        // 使用实体方法关闭工单
        wo.Close();
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

        // 使用实体方法拆分工单（内部包含验证和扣减逻辑）
        var child = wo.Split(splitQty);
        await _workOrderRepo.UpdateAsync(wo);

        var created = await _workOrderRepo.AddAsync(child);

        // 子单也生成工序步骤（如果有 RoutingId）
        if (wo.RoutingId.HasValue)
        {
            var routing = await _routingRepo.GetByIdAsync(wo.RoutingId.Value);
            if (routing != null)
            {
                foreach (var step in routing.Steps.OrderBy(s => s.StepNo))
                {
                    var woStep = WorkOrderStep.Create(
                        workOrderId: created.Id,
                        stepNo: step.StepNo,
                        stepName: step.StepName,
                        plannedQty: splitQty,
                        planStartTime: wo.PlanStartTime,
                        planEndTime: wo.PlanEndTime
                    );
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

        // 使用实体方法返工（内部包含验证和扣减逻辑）
        var child = wo.Rework(reworkQty);
        await _workOrderRepo.UpdateAsync(wo);

        var created = await _workOrderRepo.AddAsync(child);

        // 子单也生成工序步骤
        if (wo.RoutingId.HasValue)
        {
            var routing = await _routingRepo.GetByIdAsync(wo.RoutingId.Value);
            if (routing != null)
            {
                foreach (var step in routing.Steps.OrderBy(s => s.StepNo))
                {
                    var woStep = WorkOrderStep.Create(
                        workOrderId: created.Id,
                        stepNo: step.StepNo,
                        stepName: step.StepName,
                        plannedQty: reworkQty,
                        planStartTime: wo.PlanStartTime,
                        planEndTime: wo.PlanEndTime
                    );
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

        // 使用实体方法报废（内部包含验证逻辑）
        wo.Scrap(scrapQty, remark);
        await _workOrderRepo.UpdateAsync(wo);
    }
}
