using MES.Domain.Enums;

namespace MES.Domain.Entities;

public class WorkReport : BaseEntity
{
    internal WorkReport() { }

    public static WorkReport Create(
        long workOrderId,
        ReportType reportType,
        decimal goodQty,
        decimal scrapQty = 0,
        decimal reworkQty = 0,
        long? stepId = null,
        long? workstationId = null,
        long? operatorId = null,
        string? remark = null)
    {
        return new WorkReport
        {
            WorkOrderId = workOrderId,
            StepId = stepId,
            WorkstationId = workstationId,
            OperatorId = operatorId,
            ReportType = reportType,
            GoodQty = goodQty,
            ScrapQty = scrapQty,
            ReworkQty = reworkQty,
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
    public decimal GoodQty { get; set; }
    public decimal ScrapQty { get; set; }
    public decimal ReworkQty { get; set; }
    public int DurationMin { get; set; }
    public DateTime ReportTime { get; set; }
    public string? Remark { get; set; }
    public string? BatchNo { get; set; }

    public virtual WorkOrder? WorkOrder { get; set; }
    public virtual WorkOrderStep? WorkOrderStep { get; set; }
    public virtual Workstation? Workstation { get; set; }
}
