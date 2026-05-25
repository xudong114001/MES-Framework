using MES.Domain.Enums;

namespace MES.Domain.Entities;

public class MaintenancePlan : BaseEntity
{
    public long EquipmentId { get; set; }
    public string PlanName { get; set; } = string.Empty;
    public int CycleDays { get; set; }
    public DateTime? LastCompletedDate { get; set; }
    public DateTime NextDueDate { get; set; }
    public string? Description { get; set; }
    public MaintenancePlanStatus Status { get; set; } = MaintenancePlanStatus.PENDING;

    public virtual Equipment? Equipment { get; set; }
}
