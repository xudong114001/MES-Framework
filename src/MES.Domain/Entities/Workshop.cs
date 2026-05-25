namespace MES.Domain.Entities;

public class Workshop : BaseEntity
{
    public long FactoryId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool Status { get; set; } = true;

    public virtual Factory? Factory { get; set; }
    public virtual ICollection<ProductionLine> ProductionLines { get; set; } = new List<ProductionLine>();
}
