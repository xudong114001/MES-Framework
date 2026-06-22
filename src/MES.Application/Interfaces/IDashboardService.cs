namespace MES.Application.Interfaces;

public interface IDashboardService
{
    /// <summary>今日工单统计</summary>
    Task<object> GetTodayOrderStatsAsync();
    
    /// <summary>工单状态分布</summary>
    Task<object> GetOrderStatusDistributionAsync();
    
    /// <summary>产量统计</summary>
    Task<object> GetOutputStatsAsync();
    
    /// <summary>设备状态分布</summary>
    Task<object> GetEquipmentStatusAsync();
}
