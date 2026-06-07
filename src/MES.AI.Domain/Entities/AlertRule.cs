using MES.AI.Domain.Enums;

namespace MES.AI.Domain.Entities;

public class AlertRule
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Condition { get; set; } = string.Empty;
    public AlertLevel Level { get; set; }
    public bool IsEnabled { get; set; } = true;
}
