using MES.Domain.Enums;

namespace MES.Domain.Entities;

/// <summary>
/// 工序质检点配置实体
/// </summary>
public class QcCheckpoint : BaseEntity
{
    protected internal QcCheckpoint() { }

    /// <summary>工序ID（对应 RoutingStep.Id 或 WorkOrderStep.Id）</summary>
    public long StepId { get; set; }

    /// <summary>质检类型</summary>
    public QcInspectionType CheckType { get; set; }

    /// <summary>是否强制（强制质检点必须在报工前完成质检）</summary>
    public bool IsMandatory { get; set; }

    /// <summary>备注</summary>
    public string? Remark { get; set; }
}
