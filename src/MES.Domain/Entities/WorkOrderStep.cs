using MES.Domain.Enums;

namespace MES.Domain.Entities;

public class WorkOrderStep : BaseEntity
{
    public long WorkOrderId { get; set; }
    public int StepNo { get; set; }
    public string StepName { get; set; } = string.Empty;
    public long? WorkstationId { get; set; }
    public decimal PlannedQty { get; set; }
    public decimal CompletedQty { get; set; }
    public decimal ScrapQty { get; set; }
    public WorkOrderStatus Status { get; set; }
    public DateTime? PlanStartTime { get; set; }
    public DateTime? PlanEndTime { get; set; }

    public virtual WorkOrder? WorkOrder { get; set; }
    public virtual Workstation? Workstation { get; set; }
}
