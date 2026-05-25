namespace MES.Domain.Entities;

public class Bom : BaseEntity
{
    public long ProductId { get; set; }
    public long MaterialId { get; set; }
    public decimal Quantity { get; set; }
    public decimal ScrapRate { get; set; }
    public int SeqNo { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public bool Status { get; set; } = true;

    public virtual Material? Product { get; set; }
    public virtual Material? Material { get; set; }
}
