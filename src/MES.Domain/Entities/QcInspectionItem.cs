using MES.Domain.Enums;
using MES.Domain.Exceptions;

namespace MES.Domain.Entities;

public class QcInspectionItem : BaseEntity
{
    public long InspectionId { get; internal set; }
    public string ItemName { get; private set; } = string.Empty;
    public string? SpecValue { get; private set; }
    public string? ActualValue { get; private set; }
    public QcResult Result { get; private set; }

    public virtual QcInspection? QcInspection { get; set; }

    /// <summary>
    /// EF Core 需要的无参构造函数
    /// </summary>
    protected QcInspectionItem() { }

    /// <summary>
    /// 创建质检项
    /// </summary>
    public QcInspectionItem(string itemName, string? specValue = null, string? actualValue = null, QcResult result = QcResult.PENDING)
    {
        if (string.IsNullOrWhiteSpace(itemName))
            throw new DomainException("质检项名称不能为空");

        ItemName = itemName;
        SpecValue = specValue;
        ActualValue = actualValue;
        Result = result;
    }

    /// <summary>
    /// 判定质检项结果
    /// </summary>
    public void SetResult(QcResult result, string? actualValue = null)
    {
        Result = result;
        if (actualValue != null)
            ActualValue = actualValue;
    }
}
