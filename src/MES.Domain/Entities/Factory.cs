namespace MES.Domain.Entities;

public class Factory : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public bool Status { get; set; } = true;

    public virtual ICollection<Workshop> Workshops { get; set; } = new List<Workshop>();
}
