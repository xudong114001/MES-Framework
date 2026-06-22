using MES.Domain.Enums;
using MES.Domain.Exceptions;

namespace MES.Domain.Entities;

public class WorkOrderStep : BaseEntity
{
    public long WorkOrderId { get; private set; }
    public int StepNo { get; private set; }
    public string StepName { get; private set; } = string.Empty;
    public long? WorkstationId { get; private set; }
    public decimal PlannedQty { get; private set; }
    public decimal CompletedQty { get; private set; }
    public decimal ScrapQty { get; private set; }
    public WorkOrderStatus Status { get; private set; }
    public DateTime? PlanStartTime { get; private set; }
    public DateTime? PlanEndTime { get; private set; }

    public virtual WorkOrder? WorkOrder { get; set; }
    public virtual Workstation? Workstation { get; set; }

    /// <summary>
    /// EF Core 需要的无参构造函数
    /// </summary>
    [System.Text.Json.Serialization.JsonConstructor]
    internal WorkOrderStep() { }

    /// <summary>
    /// 创建工序步骤
    /// </summary>
    public static WorkOrderStep Create(
        long workOrderId,
        int stepNo,
        string stepName,
        decimal plannedQty,
        DateTime? planStartTime = null,
        DateTime? planEndTime = null,
        long? workstationId = null)
    {
        if (string.IsNullOrWhiteSpace(stepName))
            throw new DomainException("工序名称不能为空");

        if (plannedQty <= 0)
            throw new DomainException("计划数量必须大于0");

        return new WorkOrderStep
        {
            WorkOrderId = workOrderId,
            StepNo = stepNo,
            StepName = stepName,
            WorkstationId = workstationId,
            PlannedQty = plannedQty,
            CompletedQty = 0,
            ScrapQty = 0,
            Status = WorkOrderStatus.PENDING,
            PlanStartTime = planStartTime,
            PlanEndTime = planEndTime
        };
    }

    /// <summary>
    /// 开始工序：PENDING/SCHEDULED → IN_PROGRESS
    /// </summary>
    public void Start()
    {
        EnsureStatusOneOf([WorkOrderStatus.PENDING, WorkOrderStatus.SCHEDULED], "只有 PENDING/SCHEDULED 状态的工序才能开始");
        Status = WorkOrderStatus.IN_PROGRESS;
    }

    /// <summary>
    /// 完成工序：IN_PROGRESS → COMPLETED
    /// </summary>
    public void Complete()
    {
        EnsureStatus(WorkOrderStatus.IN_PROGRESS, "只有 IN_PROGRESS 状态的工序才能完成");
        Status = WorkOrderStatus.COMPLETED;
    }

    /// <summary>
    /// 暂停工序：IN_PROGRESS → ON_HOLD
    /// </summary>
    public void Hold()
    {
        EnsureStatus(WorkOrderStatus.IN_PROGRESS, "只有 IN_PROGRESS 状态的工序才能暂停");
        Status = WorkOrderStatus.ON_HOLD;
    }

    /// <summary>
    /// 恢复工序：ON_HOLD → IN_PROGRESS
    /// </summary>
    public void Resume()
    {
        EnsureStatus(WorkOrderStatus.ON_HOLD, "只有 ON_HOLD 状态的工序才能恢复");
        Status = WorkOrderStatus.IN_PROGRESS;
    }

    /// <summary>
    /// 分配工作站
    /// </summary>
    public void AssignWorkstation(long workstationId)
    {
        if (workstationId <= 0)
            throw new DomainException("工作站ID无效");
        WorkstationId = workstationId;
    }

    /// <summary>
    /// 取消工作站分配
    /// </summary>
    public void UnassignWorkstation()
    {
        WorkstationId = null;
    }

    /// <summary>
    /// 更新工序进度
    /// </summary>
    public void UpdateProgress(decimal goodQty, decimal scrapQty)
    {
        EnsureStatusOneOf([WorkOrderStatus.RELEASED, WorkOrderStatus.IN_PROGRESS, WorkOrderStatus.SCHEDULED], "工单状态不允许更新进度");

        if (goodQty < 0 || scrapQty < 0)
            throw new DomainException("数量不能为负数");

        // RELEASED 状态首次更新时自动转为 IN_PROGRESS
        if (Status == WorkOrderStatus.RELEASED)
        {
            Status = WorkOrderStatus.IN_PROGRESS;
        }

        CompletedQty += goodQty;
        ScrapQty += scrapQty;

        // 如果完工+报废达到计划数量，自动完成
        if (CompletedQty + ScrapQty >= PlannedQty)
        {
            Status = WorkOrderStatus.COMPLETED;
        }
    }

    /// <summary>
    /// 更新计划时间
    /// </summary>
    public void UpdatePlannedTimes(DateTime? planStartTime, DateTime? planEndTime)
    {
        PlanStartTime = planStartTime;
        PlanEndTime = planEndTime;
    }

    private void EnsureStatus(WorkOrderStatus expected, string message)
    {
        if (Status != expected)
            throw new DomainException(message);
    }

    private void EnsureStatusOneOf(WorkOrderStatus[] expectedStatuses, string message)
    {
        if (!expectedStatuses.Contains(Status))
            throw new DomainException(message);
    }
}