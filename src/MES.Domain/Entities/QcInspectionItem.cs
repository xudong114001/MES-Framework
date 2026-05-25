using MES.Domain.Enums;

namespace MES.Domain.Entities;

public class QcInspectionItem : BaseEntity
{
    public long InspectionId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string? SpecValue { get; set; }
    public string? ActualValue { get; set; }
    public QcResult Result { get; set; }

    public virtual QcInspection? QcInspection { get; set; }
}
