using MES.Domain.Enums;

namespace MES.Domain.Entities;

public class QcInspection : BaseEntity
{
    public string InspectNo { get; set; } = string.Empty;
    public QcInspectionType SourceType { get; set; }
    public string? SourceRef { get; set; }
    public long? WorkOrderId { get; set; }
    public long? MaterialId { get; set; }
    public long? Inspector { get; set; }
    public QcResult InspectResult { get; set; }
    public DateTime InspectTime { get; set; }
    public string? Remark { get; set; }

    /// <summary>
    /// 不合格品处理动作: CONCESSION(让步接收), REWORK(返工), SCRAP(报废)
    /// </summary>
    public string? HandlingAction { get; set; }
    /// <summary>
    /// 不合格品处理备注
    /// </summary>
    public string? HandlingRemark { get; set; }
    /// <summary>
    /// 不合格品处理时间
    /// </summary>
    public DateTime? HandledAt { get; set; }

    public virtual ICollection<QcInspectionItem> Items { get; set; } = new List<QcInspectionItem>();
}
