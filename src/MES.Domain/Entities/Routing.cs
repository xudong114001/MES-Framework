namespace MES.Domain.Entities;

public class Routing : BaseEntity
{
    public long MaterialId { get; set; }
    public string RoutingCode { get; set; } = string.Empty;
    public string RoutingName { get; set; } = string.Empty;
    public string Version { get; set; } = "V1.0";
    public bool Status { get; set; } = true;

    public virtual Material? Material { get; set; }
    public virtual ICollection<RoutingStep> Steps { get; set; } = new List<RoutingStep>();
}
