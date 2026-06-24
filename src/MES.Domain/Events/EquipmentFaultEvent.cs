using MES.Domain.Enums;

namespace MES.Domain.Events;

public class EquipmentFaultEvent : DomainEvent
{
    public EquipmentFaultEvent(long equipmentId, string equipmentCode, string equipmentName, EquipmentStatus oldStatus)
    {
        EquipmentId = equipmentId;
        EquipmentCode = equipmentCode;
        EquipmentName = equipmentName;
        OldStatus = oldStatus;
    }

    public long EquipmentId { get; init; }
    public string EquipmentCode { get; init; } = string.Empty;
    public string EquipmentName { get; init; } = string.Empty;
    public EquipmentStatus OldStatus { get; init; }
}
