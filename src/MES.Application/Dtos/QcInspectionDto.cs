using MES.Domain.Enums;

namespace MES.Application.Dtos;

/// <summary>
/// 质检 DTO — 排除导航属性，仅暴露数据传输所需字段
/// </summary>
public class QcInspectionDto
{
    public long Id { get; set; }
    public string InspectNo { get; set; } = string.Empty;
    public QcInspectionType SourceType { get; set; }
    public string? SourceRef { get; set; }
    public long? WorkOrderId { get; set; }
    public long? MaterialId { get; set; }
    public long? Inspector { get; set; }
    public QcResult InspectResult { get; set; }
    public DateTime InspectTime { get; set; }
    public string? Remark { get; set; }
    public string? HandlingAction { get; set; }
    public string? HandlingRemark { get; set; }
    public DateTime? HandledAt { get; set; }

    public DateTime CreatedAt { get; set; }
    public long? CreatedBy { get; set; }
    public DateTime UpdatedAt { get; set; }
    public long? UpdatedBy { get; set; }
}
