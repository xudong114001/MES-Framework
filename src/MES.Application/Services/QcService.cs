using Microsoft.Extensions.Logging;
using MES.Application.Interfaces;
using MES.Application.Integration.Events;
using MES.Domain.Entities;
using MES.Domain.Enums;
using MES.Domain.Exceptions;
using MES.Infrastructure.Repositories;

namespace MES.Application.Services;

public class QcService
{
    private readonly IRepository<QcInspection> _inspectionRepo;
    private readonly IRepository<QcInspectionItem> _itemRepo;
    private readonly IRepository<WorkOrder> _workOrderRepo1;
    private readonly IRepository<WorkOrderStep> _stepRepo;
    private readonly IEventBus? _eventBus;
    private readonly InMemoryEventLogService? _eventLog;
    private readonly ILogger<QcService>? _logger;

    public QcService(
        IRepository<QcInspection> inspectionRepo,
        IRepository<QcInspectionItem> itemRepo,
        IRepository<WorkOrder> workOrderRepo,
        IRepository<WorkOrderStep> stepRepo,
        IEventBus? eventBus = null,
        InMemoryEventLogService? eventLog = null,
        ILogger<QcService>? logger = null)
    {
        _inspectionRepo = inspectionRepo;
        _itemRepo = itemRepo;
        _workOrderRepo1 = workOrderRepo;
        _stepRepo = stepRepo;
        _eventBus = eventBus;
        _eventLog = eventLog;
        _logger = logger;
    }

    /// <summary>
    /// 创建质检单
    /// </summary>
    public async Task<QcInspection> CreateInspectionAsync(string inspectNo, QcInspectionType sourceType, long? workOrderId = null, long? materialId = null, long? inspector = null, string? sourceRef = null, string? remark = null)
    {
        var inspection = QcInspection.Create(inspectNo, sourceType, workOrderId, materialId, inspector, sourceRef, remark);
        return await _inspectionRepo.AddAsync(inspection);
    }

    /// <summary>
    /// 添加质检项
    /// </summary>
    public async Task<QcInspectionItem> AddItemAsync(long inspectionId, string itemName, string? specValue = null)
    {
        var inspection = await _inspectionRepo.GetByIdAsync(inspectionId);
        if (inspection == null)
            throw new DomainException("质检单不存在");

        var item = new QcInspectionItem(itemName, specValue);
        inspection.AddItem(item);
        await _inspectionRepo.UpdateAsync(inspection);

        return item;
    }

    /// <summary>
    /// 判定质检结果
    /// </summary>
    public async Task VerifyInspectionAsync(long inspectionId, QcResult result)
    {
        var inspection = await _inspectionRepo.GetByIdAsync(inspectionId);
        if (inspection == null)
            throw new DomainException("质检单不存在");

        // 使用领域方法
        inspection.Verify(result);
        await _inspectionRepo.UpdateAsync(inspection);

        // 发布质检完成事件（失败不影响主事务）
        try
        {
            if (_eventBus != null)
            {
                var evt = new QcInspectionCompletedEvent
                {
                    InspectionId = inspection.Id,
                    InspectNo = inspection.InspectNo,
                    WorkOrderId = inspection.WorkOrderId,
                    MaterialId = inspection.MaterialId,
                    Result = result,
                    HandlingAction = inspection.HandlingAction,
                    HandledAt = inspection.HandledAt
                };
                await _eventBus.Publish(evt);
                _eventLog?.Log(evt, "Published");
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to publish QcInspectionCompletedEvent");
            _eventLog?.Log(new QcInspectionCompletedEvent
            {
                InspectionId = inspection.Id,
                InspectNo = inspection.InspectNo,
                WorkOrderId = inspection.WorkOrderId,
                MaterialId = inspection.MaterialId,
                Result = result
            }, "Failed", ex.Message);
        }

        // 质检不通过时，暂停该工单下游所有未开始的工序
        if (result == QcResult.FAIL && inspection.WorkOrderId.HasValue)
        {
            var steps = (await _stepRepo.FindAsync(s => s.WorkOrderId == inspection.WorkOrderId.Value))
                .OrderBy(s => s.StepNo)
                .ToList();

            if (steps.Any())
            {
                // 尝试从 SourceRef 定位当前质检对应的工序
                WorkOrderStep? currentStep = null;
                if (!string.IsNullOrEmpty(inspection.SourceRef))
                {
                    currentStep = steps.FirstOrDefault(s =>
                        inspection.SourceRef.Contains(s.StepName, StringComparison.OrdinalIgnoreCase));
                }

                if (currentStep != null)
                {
                    var downstreamSteps = steps
                        .Where(s => s.StepNo > currentStep.StepNo && s.Status == WorkOrderStatus.PENDING)
                        .ToList();

                    foreach (var downstream in downstreamSteps)
                    {
                        downstream.Hold();
                    }

                    foreach (var downstream in downstreamSteps)
                    {
                        await _stepRepo.UpdateAsync(downstream);
                    }
                }
            }
        }
    }

    /// <summary>
    /// 不合格品处理
    /// </summary>
    /// <param name="inspectionId">质检单ID</param>
    /// <param name="action">处理动作: CONCESSION(让步接收), REWORK(返工), SCRAP(报废)</param>
    /// <param name="remark">处理备注</param>
    public async Task HandleNonconformingAsync(long inspectionId, string action, string? remark)
    {
        var inspection = await _inspectionRepo.GetByIdAsync(inspectionId);
        if (inspection == null)
            throw new DomainException("质检单不存在");

        // 使用领域方法
        inspection.HandleNonconforming(action, remark);

        // 根据处理动作更新相关数据
        switch (action)
        {
            case "SCRAP":
                await HandleScrapAsync(inspection);
                break;
            case "REWORK":
                await HandleReworkAsync(inspection);
                break;
            case "CONCESSION":
                // 让步接收：仅记录处理信息，无需更新数量
                break;
        }

        await _inspectionRepo.UpdateAsync(inspection);
    }

    private async Task HandleScrapAsync(QcInspection inspection)
    {
        if (!inspection.WorkOrderId.HasValue)
            return;

        var wo = await _workOrderRepo1.GetByIdAsync(inspection.WorkOrderId.Value);
        if (wo == null) return;

        // 使用领域方法增加报废数量
        wo.AddScrap(1); // 增加一个单位的报废数量
        await _workOrderRepo1.UpdateAsync(wo);
    }

    private async Task HandleReworkAsync(QcInspection inspection)
    {
        if (!inspection.WorkOrderId.HasValue)
            return;

        var wo = await _workOrderRepo1.GetByIdAsync(inspection.WorkOrderId.Value);
        if (wo == null) return;

        // 使用领域方法将工单状态恢复为 IN_PROGRESS
        wo.MarkInProgress();
        await _workOrderRepo1.UpdateAsync(wo);
    }
}
