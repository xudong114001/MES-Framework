using MES.Domain.Enums;

namespace MES.Domain.Entities;

public class AlertRule : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Condition { get; set; } = string.Empty;
    public AlertLevel Level { get; set; }
    public bool IsEnabled { get; set; } = true;
}
