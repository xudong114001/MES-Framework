using MES.Domain.AggregateRoots;
using MES.Domain.Enums;
using MES.Domain.Exceptions;

namespace MES.Domain.Entities;

public class QcInspection : BaseEntity, IAggregateRoot
{
    private readonly List<QcInspectionItem> _items = new();

    public string InspectNo { get; private set; } = string.Empty;
    public QcInspectionType SourceType { get; private set; }
    public string? SourceRef { get; private set; }
    public long? WorkOrderId { get; private set; }
    public long? MaterialId { get; private set; }
    public long? Inspector { get; private set; }
    public QcResult InspectResult { get; private set; }
    public DateTime InspectTime { get; private set; }
    public string? Remark { get; private set; }

    /// <summary>
    /// 不合格品处理动作: CONCESSION(让步接收), REWORK(返工), SCRAP(报废)
    /// </summary>
    public InspectionResult? HandlingAction { get; private set; }
    /// <summary>
    /// 不合格品处理备注
    /// </summary>
    public string? HandlingRemark { get; private set; }
    /// <summary>
    /// 不合格品处理时间
    /// </summary>
    public DateTime? HandledAt { get; private set; }

    public IReadOnlyCollection<QcInspectionItem> Items => _items.AsReadOnly();

    /// <summary>
    /// EF Core 需要的无参构造函数
    /// </summary>
    protected QcInspection() { }

    #region 工厂方法

    /// <summary>
    /// 创建质检单，默认状态 PENDING
    /// </summary>
    public static QcInspection Create(
        string inspectNo,
        QcInspectionType sourceType,
        long? workOrderId = null,
        long? materialId = null,
        long? inspector = null,
        string? sourceRef = null,
        string? remark = null)
    {
        if (string.IsNullOrWhiteSpace(inspectNo))
            throw new DomainException("质检单号不能为空");

        return new QcInspection
        {
            InspectNo = inspectNo,
            SourceType = sourceType,
            SourceRef = sourceRef,
            WorkOrderId = workOrderId,
            MaterialId = materialId,
            Inspector = inspector,
            InspectResult = QcResult.PENDING,
            Remark = remark
        };
    }

    #endregion

    #region 行为方法

    /// <summary>
    /// 判定质检结果：只有 PENDING 状态才能判定
    /// </summary>
    public void Verify(QcResult result)
    {
        if (InspectResult != QcResult.PENDING)
            throw new DomainException("只有待检状态的质检单才能判定");

        InspectResult = result;
        InspectTime = DateTime.UtcNow;
    }

    /// <summary>
    /// 不合格品处理：只有 FAIL 状态才能处理
    /// </summary>
    /// <param name="action">处理动作: CONCESSION(让步接收), REWORK(返工), SCRAP(报废)</param>
    /// <param name="remark">处理备注</param>
    public void HandleNonconforming(InspectionResult action, string? remark)
    {
        if (InspectResult != QcResult.FAIL)
            throw new DomainException("只有不合格的质检单才能进行不合格品处理");

        if (action != InspectionResult.REWORK && action != InspectionResult.SCRAP && action != InspectionResult.CONCESSION)
            throw new DomainException($"无效的处理动作，可选值: REWORK, SCRAP, CONCESSION");

        HandlingAction = action;
        HandlingRemark = remark;
        HandledAt = DateTime.UtcNow;
    }

    /// <summary>
    /// 添加质检项
    /// </summary>
    public void AddItem(QcInspectionItem item)
    {
        if (item == null)
            throw new DomainException("质检项不能为空");

        item.InspectionId = Id;
        _items.Add(item);
    }

    /// <summary>
    /// 移除质检项
    /// </summary>
    public void RemoveItem(QcInspectionItem item)
    {
        if (item == null)
            throw new DomainException("质检项不能为空");

        _items.Remove(item);
    }

    #endregion
}
