namespace MES.Domain.Entities;

public class RoutingStep : BaseEntity
{
    public long RoutingId { get; set; }
    public int StepNo { get; set; }
    public string StepName { get; set; } = string.Empty;
    public string? WorkstationType { get; set; }
    public decimal StandardTime { get; set; }
    public bool IsQcPoint { get; set; }
    public bool IsScrapPoint { get; set; }

    public virtual Routing? Routing { get; set; }
}
