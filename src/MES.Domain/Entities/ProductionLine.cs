using MES.Domain.Enums;

namespace MES.Domain.Entities;

public class ProductionLine : BaseEntity
{
    public long WorkshopId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public LineType LineType { get; set; }
    public bool Status { get; set; } = true;

    public virtual Workshop? Workshop { get; set; }
    public virtual ICollection<Workstation> Workstations { get; set; } = new List<Workstation>();
}
