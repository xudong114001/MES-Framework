namespace MES.Domain.Entities;

public class Workstation : BaseEntity
{
    public long LineId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int SeqNo { get; set; }
    public bool Status { get; set; } = true;

    public virtual ProductionLine? ProductionLine { get; set; }
}
