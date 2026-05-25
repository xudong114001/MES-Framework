using MES.Domain.Enums;

namespace MES.Domain.Entities;

public class WorkOrder : BaseEntity
{
    public string OrderNo { get; set; } = string.Empty;
    public SourceType SourceType { get; set; }
    public string? SourceRef { get; set; }
    public long MaterialId { get; set; }
    public long? RoutingId { get; set; }
    public decimal PlannedQty { get; set; }
    public decimal CompletedQty { get; set; }
    public decimal ScrapQty { get; set; }
    public WorkOrderStatus Status { get; set; }
    public DateTime? PlanStartTime { get; set; }
    public DateTime? PlanEndTime { get; set; }
    public DateTime? ActualStartTime { get; set; }
    public DateTime? ActualEndTime { get; set; }
    public Priority Priority { get; set; } = Priority.NORMAL;
    public long? FactoryId { get; set; }
    public long? WorkshopId { get; set; }
    public long? LineId { get; set; }
    public long? Assignee { get; set; }
    public string? Remark { get; set; }
    /// <summary>从哪个工单返工而来</summary>
    public long? ReworkFromId { get; set; }

    public virtual Material? Material { get; set; }
    public virtual Routing? Routing { get; set; }
    public virtual ICollection<WorkOrderStep> Steps { get; set; } = new List<WorkOrderStep>();
}
