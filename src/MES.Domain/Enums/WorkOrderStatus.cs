namespace MES.Domain.Enums;

public enum WorkOrderStatus
{
    PENDING,      // 待下达
    RELEASED,     // 已下达
    SCHEDULED,    // 已排产
    IN_PROGRESS,  // 生产中
    COMPLETED,    // 已完成
    CLOSED,       // 已关闭
    CANCELLED,    // 已取消
    ON_HOLD       // 暂停
}
