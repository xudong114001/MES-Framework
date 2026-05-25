namespace MES.Domain.Entities;

public class MaterialTrace : BaseEntity
{
    public long MaterialId { get; set; }
    public string? BatchNo { get; set; }
    public string? SerialNo { get; set; }
    public long? WorkOrderId { get; set; }
    public long? SrcWorkOrderId { get; set; }
    public string? Direction { get; set; }
    public decimal Qty { get; set; }
    public long? Operator { get; set; }
    public DateTime OperateTime { get; set; }
    public string? Remark { get; set; }

    public virtual Material? Material { get; set; }
}
