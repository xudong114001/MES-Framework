using MES.Domain.Enums;

namespace MES.Application.Dtos;

/// <summary>
/// 报工 DTO — 排除导航属性，仅暴露数据传输所需字段
/// </summary>
public class WorkReportDto
{
    public long Id { get; set; }
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

    public DateTime CreatedAt { get; set; }
    public long? CreatedBy { get; set; }
    public DateTime UpdatedAt { get; set; }
    public long? UpdatedBy { get; set; }
}
