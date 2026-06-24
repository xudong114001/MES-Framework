namespace MES.Domain.Enums;

/// <summary>
/// Andon 事件类型
/// </summary>
public enum AndonEventType
{
    QUALITY_ALARM,     // 质量异常
    EQUIPMENT_FAULT,   // 设备故障
    MATERIAL_SHORTAGE, // 物料短缺
    PRODUCTION_DELAY,  // 生产延迟
    OTHER              // 其他异常
}
