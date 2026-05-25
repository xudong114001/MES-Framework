using MES.Domain.Entities;
using MES.Domain.Enums;
using MES.Infrastructure.Repositories;

namespace MES.Application.Services;

public class QcService
{
    private readonly IRepository<QcInspection> _inspectionRepo;
    private readonly IRepository<QcInspectionItem> _itemRepo;
    private readonly IRepository<WorkOrder> _workOrderRepo1;
    private readonly IRepository<WorkOrderStep> _stepRepo;

    public QcService(
        IRepository<QcInspection> inspectionRepo,
        IRepository<QcInspectionItem> itemRepo,
        IRepository<WorkOrder> workOrderRepo,
        IRepository<WorkOrderStep> stepRepo)
    {
        _inspectionRepo = inspectionRepo;
        _itemRepo = itemRepo;
        _workOrderRepo1 = workOrderRepo;
        _stepRepo = stepRepo;
    }

    /// <summary>
    /// 创建质检单
    /// </summary>
    public async Task<QcInspection> CreateInspectionAsync(QcInspection inspection)
    {
        inspection.InspectResult = QcResult.PENDING;
        return await _inspectionRepo.AddAsync(inspection);
    }

    /// <summary>
    /// 添加质检项
    /// </summary>
    public async Task<QcInspectionItem> AddItemAsync(QcInspectionItem item)
    {
        return await _itemRepo.AddAsync(item);
    }

    /// <summary>
    /// 判定质检结果
    /// </summary>
    public async Task VerifyInspectionAsync(long inspectionId, QcResult result)
    {
        var inspection = await _inspectionRepo.GetByIdAsync(inspectionId);
        if (inspection == null) throw new InvalidOperationException("质检单不存在");
        inspection.InspectResult = result;
        inspection.InspectTime = DateTime.UtcNow;
        await _inspectionRepo.UpdateAsync(inspection);

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
                        downstream.Status = WorkOrderStatus.ON_HOLD;
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
            throw new InvalidOperationException("质检单不存在");

        if (inspection.InspectResult != QcResult.FAIL)
            throw new InvalidOperationException("只有不合格的质检单才能进行不合格品处理");

        var validActions = new[] { "CONCESSION", "REWORK", "SCRAP" };
        if (!validActions.Contains(action))
            throw new InvalidOperationException($"无效的处理动作，可选值: {string.Join(", ", validActions)}");

        inspection.HandlingAction = action;
        inspection.HandlingRemark = remark;
        inspection.HandledAt = DateTime.UtcNow;

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
                // 让步接收：仅记录处理信息，无需跟新数量
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

        // 估算报废数量：如果质检单关联了具体的检验数量，这里以不合格品项数为参考
        // 简单实现：将工单未完成的部分标记为报废
        wo.ScrapQty += 1; // 增加一个单位的报废数量
        await _workOrderRepo1.UpdateAsync(wo);
    }

    private async Task HandleReworkAsync(QcInspection inspection)
    {
        if (!inspection.WorkOrderId.HasValue)
            return;

        var wo = await _workOrderRepo1.GetByIdAsync(inspection.WorkOrderId.Value);
        if (wo == null) return;

        // 返工：标记工单状态为 IN_PROGRESS（如果已完成的证则变回进行中）
        if (wo.Status == WorkOrderStatus.COMPLETED)
        {
            wo.Status = WorkOrderStatus.IN_PROGRESS;
            await _workOrderRepo1.UpdateAsync(wo);
        }
    }
}
