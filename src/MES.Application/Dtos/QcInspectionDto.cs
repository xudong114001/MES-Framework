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
    public InspectionResult? HandlingAction { get; set; }
    public string? HandlingRemark { get; set; }
    public DateTime? HandledAt { get; set; }

    public DateTime CreatedAt { get; set; }
    public long? CreatedBy { get; set; }
    public DateTime UpdatedAt { get; set; }
    public long? UpdatedBy { get; set; }
}

/// <summary>
/// 判定质检结果请求
/// </summary>
public class VerifyRequest
{
    public QcResult Result { get; set; }
}

/// <summary>
/// 不合格品处理请求
/// </summary>
public class HandleNonconformingRequest
{
    /// <summary>处理动作: CONCESSION(让步接收), REWORK(返工), SCRAP(报废)</summary>
    public InspectionResult Action { get; set; }
    /// <summary>处理备注</summary>
    public string? Remark { get; set; }
}

/// <summary>
/// 创建质检单请求
/// </summary>
public class CreateInspectionRequest
{
    public string InspectNo { get; set; } = string.Empty;
    public QcInspectionType SourceType { get; set; }
    public long? WorkOrderId { get; set; }
    public long? MaterialId { get; set; }
    public long? Inspector { get; set; }
    public string? SourceRef { get; set; }
    public string? Remark { get; set; }
}

/// <summary>
/// 添加质检项请求
/// </summary>
public class AddItemRequest
{
    public string ItemName { get; set; } = string.Empty;
    public string? SpecValue { get; set; }
}
