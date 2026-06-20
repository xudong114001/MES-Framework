using MES.Domain.AggregateRoots;
using MES.Domain.Enums;
using MES.Domain.Exceptions;

namespace MES.Domain.Entities;

public class WorkOrder : BaseEntity, IAggregateRoot
{
    public string OrderNo { get; private set; } = string.Empty;
    public SourceType SourceType { get; private set; }
    public string? SourceRef { get; private set; }
    public long MaterialId { get; private set; }
    public long? RoutingId { get; private set; }
    public decimal PlannedQty { get; private set; }
    public decimal CompletedQty { get; private set; }
    public decimal ScrapQty { get; private set; }
    public WorkOrderStatus Status { get; private set; }
    public DateTime? PlanStartTime { get; private set; }
    public DateTime? PlanEndTime { get; private set; }
    public DateTime? ActualStartTime { get; private set; }
    public DateTime? ActualEndTime { get; private set; }
    public Priority Priority { get; private set; } = Priority.NORMAL;
    public long? FactoryId { get; private set; }
    public long? WorkshopId { get; private set; }
    public long? LineId { get; private set; }
    public long? Assignee { get; private set; }
    public string? Remark { get; private set; }
    /// <summary>从哪个工单返工而来</summary>
    public long? ReworkFromId { get; private set; }

    public virtual Material? Material { get; set; }
    public virtual Routing? Routing { get; set; }
    public virtual ICollection<WorkOrderStep> Steps { get; set; } = new List<WorkOrderStep>();

    /// <summary>
    /// EF Core 需要的无参构造函数
    /// </summary>
    protected WorkOrder() { }

    #region 工厂方法

    /// <summary>
    /// 创建工单，默认状态 PENDING
    /// </summary>
    public static WorkOrder Create(
        string orderNo,
        SourceType sourceType,
        long materialId,
        decimal plannedQty,
        Priority priority = Priority.NORMAL,
        long? routingId = null,
        string? sourceRef = null,
        DateTime? planStartTime = null,
        DateTime? planEndTime = null,
        long? factoryId = null,
        long? workshopId = null,
        long? lineId = null,
        long? assignee = null,
        string? remark = null,
        long? reworkFromId = null)
    {
        if (string.IsNullOrWhiteSpace(orderNo))
            throw new DomainException("工单号不能为空");

        if (materialId <= 0)
            throw new DomainException("物料ID无效");

        if (plannedQty <= 0)
            throw new DomainException("计划数量必须大于0");

        return new WorkOrder
        {
            OrderNo = orderNo,
            SourceType = sourceType,
            SourceRef = sourceRef,
            MaterialId = materialId,
            RoutingId = routingId,
            PlannedQty = plannedQty,
            CompletedQty = 0,
            ScrapQty = 0,
            Status = WorkOrderStatus.PENDING,
            Priority = priority,
            PlanStartTime = planStartTime,
            PlanEndTime = planEndTime,
            FactoryId = factoryId,
            WorkshopId = workshopId,
            LineId = lineId,
            Assignee = assignee,
            Remark = remark,
            ReworkFromId = reworkFromId
        };
    }

    /// <summary>
    /// 创建拆分子工单
    /// </summary>
    public WorkOrder CreateSplitChild(decimal splitQty)
    {
        EnsureStatusOneOf([WorkOrderStatus.PENDING, WorkOrderStatus.RELEASED], "仅 PENDING/RELEASED 状态的工单允许拆分");

        if (splitQty <= 0)
            throw new DomainException("拆分数量必须大于0");

        if (splitQty >= PlannedQty)
            throw new DomainException("拆分数量必须小于原单计划数量");

        PlannedQty -= splitQty;

        return new WorkOrder
        {
            OrderNo = $"{OrderNo}-SUB",
            SourceType = SourceType,
            SourceRef = $"SplitFrom:{Id}",
            MaterialId = MaterialId,
            RoutingId = RoutingId,
            PlannedQty = splitQty,
            CompletedQty = 0,
            ScrapQty = 0,
            Status = WorkOrderStatus.PENDING,
            Priority = Priority,
            PlanStartTime = PlanStartTime,
            PlanEndTime = PlanEndTime,
            FactoryId = FactoryId,
            WorkshopId = WorkshopId,
            LineId = LineId,
            Assignee = Assignee,
            Remark = $"工单拆分自 {OrderNo}(ID={Id})"
        };
    }

    /// <summary>
    /// 创建返工子工单
    /// </summary>
    public WorkOrder CreateReworkChild(decimal reworkQty, string? remark = null)
    {
        EnsureStatusOneOf([WorkOrderStatus.COMPLETED, WorkOrderStatus.IN_PROGRESS], "仅 COMPLETED/IN_PROGRESS 状态的工单允许返工");

        if (reworkQty <= 0)
            throw new DomainException("返工数量必须大于0");

        if (reworkQty > CompletedQty)
            throw new DomainException($"返工数量({reworkQty})超过已完成数量({CompletedQty})");

        CompletedQty -= reworkQty;

        return new WorkOrder
        {
            OrderNo = $"{OrderNo}-RWK",
            SourceType = SourceType,
            SourceRef = $"ReworkFrom:{Id}",
            MaterialId = MaterialId,
            RoutingId = RoutingId,
            PlannedQty = reworkQty,
            CompletedQty = 0,
            ScrapQty = 0,
            Status = WorkOrderStatus.PENDING,
            Priority = Priority,
            PlanStartTime = PlanStartTime,
            PlanEndTime = PlanEndTime,
            FactoryId = FactoryId,
            WorkshopId = WorkshopId,
            LineId = LineId,
            Assignee = Assignee,
            Remark = remark ?? $"返工自 {OrderNo}(ID={Id})",
            ReworkFromId = Id
        };
    }

    #endregion

    #region 状态转换方法

    /// <summary>
    /// 下达工单：PENDING → RELEASED
    /// </summary>
    public void Release()
    {
        EnsureStatus(WorkOrderStatus.PENDING, "只有 PENDING 状态的工单才能下达");
        Status = WorkOrderStatus.RELEASED;
    }

    /// <summary>
    /// 排产工单：RELEASED → SCHEDULED
    /// </summary>
    public void Schedule(long lineId)
    {
        EnsureStatus(WorkOrderStatus.RELEASED, "只有 RELEASED 状态的工单才能排产");
        if (lineId <= 0)
            throw new DomainException("产线ID无效");
        LineId = lineId;
        Status = WorkOrderStatus.SCHEDULED;
    }

    /// <summary>
    /// 开始生产：SCHEDULED/RELEASED → IN_PROGRESS
    /// </summary>
    public void Start()
    {
        EnsureStatusOneOf([WorkOrderStatus.SCHEDULED, WorkOrderStatus.RELEASED], "只有 SCHEDULED/RELEASED 状态的工单才能开始生产");
        Status = WorkOrderStatus.IN_PROGRESS;
        ActualStartTime = DateTime.UtcNow;
    }

    /// <summary>
    /// 暂停工单：RELEASED / IN_PROGRESS → ON_HOLD
    /// </summary>
    public void Hold()
    {
        EnsureStatusOneOf([WorkOrderStatus.RELEASED, WorkOrderStatus.IN_PROGRESS], "只有 RELEASED/IN_PROGRESS 状态的工单才能暂停");
        Status = WorkOrderStatus.ON_HOLD;
    }

    /// <summary>
    /// 恢复工单：ON_HOLD → IN_PROGRESS
    /// </summary>
    public void Resume()
    {
        EnsureStatus(WorkOrderStatus.ON_HOLD, "只有 ON_HOLD 状态的工单才能恢复");
        Status = WorkOrderStatus.IN_PROGRESS;
    }

    /// <summary>
    /// 取消工单：PENDING / RELEASED / ON_HOLD → CANCELLED
    /// </summary>
    public void Cancel()
    {
        EnsureStatusOneOf(
            [WorkOrderStatus.PENDING, WorkOrderStatus.RELEASED, WorkOrderStatus.ON_HOLD],
            "只有 PENDING/RELEASED/ON_HOLD 状态的工单才能取消");
        Status = WorkOrderStatus.CANCELLED;
    }

    /// <summary>
    /// 完成工单：IN_PROGRESS → COMPLETED
    /// </summary>
    public void Complete()
    {
        EnsureStatus(WorkOrderStatus.IN_PROGRESS, "只有 IN_PROGRESS 状态的工单才能完成");
        Status = WorkOrderStatus.COMPLETED;
    }

    /// <summary>
    /// 关闭工单：COMPLETED → CLOSED
    /// </summary>
    public void Close()
    {
        EnsureStatus(WorkOrderStatus.COMPLETED, "只有 COMPLETED 状态的工单才能关闭");
        Status = WorkOrderStatus.CLOSED;
        ActualEndTime = DateTime.UtcNow;
    }

    /// <summary>
    /// 取消排产：SCHEDULED → RELEASED，清除产线
    /// </summary>
    public void Unschedule()
    {
        EnsureStatus(WorkOrderStatus.SCHEDULED, "只有 SCHEDULED 状态的工单才能取消排产");
        Status = WorkOrderStatus.RELEASED;
        LineId = null;
    }

    #endregion

    #region 业务行为方法

    /// <summary>
    /// 报工：增加完工/报废/返工数量
    /// </summary>
    public void ReportProgress(decimal goodQty, decimal scrapQty, decimal reworkQty)
    {
        EnsureStatusOneOf([WorkOrderStatus.IN_PROGRESS, WorkOrderStatus.SCHEDULED], "工单状态不允许报工");

        if (goodQty < 0 || scrapQty < 0 || reworkQty < 0)
            throw new DomainException("报工数量不能为负数");

        CompletedQty += goodQty + reworkQty;
        ScrapQty += scrapQty;

        // 如果完工+报废达到计划数量，自动完成
        if (CompletedQty + ScrapQty >= PlannedQty)
        {
            Status = WorkOrderStatus.COMPLETED;
        }
    }

    /// <summary>
    /// 拆分工单：扣减原单数量，返回新的子工单
    /// </summary>
    public WorkOrder Split(decimal splitQty)
    {
        return CreateSplitChild(splitQty);
    }

    /// <summary>
    /// 返工：扣减原单完工数量，返回返工子工单
    /// </summary>
    public WorkOrder Rework(decimal reworkQty)
    {
        return CreateReworkChild(reworkQty);
    }

    /// <summary>
    /// 报废：增加报废数量
    /// </summary>
    public void Scrap(decimal scrapQty, string? remark = null)
    {
        EnsureStatusOneOf([WorkOrderStatus.IN_PROGRESS, WorkOrderStatus.RELEASED], "只有 IN_PROGRESS/RELEASED 状态的工单才能报废");

        if (scrapQty <= 0)
            throw new DomainException("报废数量必须大于0");

        var remaining = PlannedQty - CompletedQty - ScrapQty;
        if (scrapQty > remaining)
            throw new DomainException($"报废数量({scrapQty})超过剩余可操作数量({remaining})");

        ScrapQty += scrapQty;
        Remark = remark ?? Remark;

        // 如果全部报废，标记取消
        if (CompletedQty + ScrapQty >= PlannedQty)
            Status = WorkOrderStatus.CANCELLED;
    }

    /// <summary>
    /// 更新计划开始/结束时间
    /// </summary>
    public void UpdatePlannedTimes(DateTime? planStartTime, DateTime? planEndTime)
    {
        PlanStartTime = planStartTime;
        PlanEndTime = planEndTime;
    }

    /// <summary>
    /// 增加报废数量（质检专用，不检查状态）
    /// </summary>
    public void AddScrap(decimal scrapQty)
    {
        if (scrapQty <= 0)
            throw new DomainException("报废数量必须大于0");

        ScrapQty += scrapQty;
    }

    /// <summary>
    /// 恢复为生产中（返工专用）
    /// </summary>
    public void MarkInProgress()
    {
        if (Status == WorkOrderStatus.COMPLETED)
        {
            Status = WorkOrderStatus.IN_PROGRESS;
        }
    }

    #endregion

    #region 私有辅助方法

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

    #endregion
}