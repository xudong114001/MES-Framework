using MES.Domain.AggregateRoots;
using MES.Domain.Enums;
using MES.Domain.ValueObjects;

namespace MES.Domain.Entities;

public class WorkReport : BaseEntity, IAggregateRoot
{
    internal WorkReport() { }

    public static WorkReport Create(
        long workOrderId,
        ReportType reportType,
        Quantity goodQty,
        Quantity? scrapQty = null,
        Quantity? reworkQty = null,
        long? stepId = null,
        long? workstationId = null,
        long? operatorId = null,
        string? remark = null)
    {
        var unit = goodQty.Unit;
        return new WorkReport
        {
            WorkOrderId = workOrderId,
            StepId = stepId,
            WorkstationId = workstationId,
            OperatorId = operatorId,
            ReportType = reportType,
            GoodQty = goodQty,
            ScrapQty = scrapQty ?? Quantity.Zero(unit),
            ReworkQty = reworkQty ?? Quantity.Zero(unit),
            ReportTime = DateTime.UtcNow,
            ReportNo = $"RP{DateTime.Now:yyyyMMddHHmmss}{Random.Shared.Next(100, 999)}",
            Remark = remark
        };
    }

    public string ReportNo { get; set; } = string.Empty;
    public long WorkOrderId { get; set; }
    public long? StepId { get; set; }
    public long? WorkstationId { get; set; }
    public long? OperatorId { get; set; }
    public ReportType ReportType { get; set; }
    public Quantity GoodQty { get; set; } = Quantity.Zero();
    public Quantity ScrapQty { get; set; } = Quantity.Zero();
    public Quantity ReworkQty { get; set; } = Quantity.Zero();
    public int DurationMin { get; set; }
    public DateTime ReportTime { get; set; }
    public string? Remark { get; set; }
    public string? BatchNo { get; set; }

    public virtual WorkOrder? WorkOrder { get; set; }
    public virtual WorkOrderStep? WorkOrderStep { get; set; }
    public virtual Workstation? Workstation { get; set; }
}
