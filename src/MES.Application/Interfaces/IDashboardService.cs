using MES.Application.Dtos;

namespace MES.Application.Interfaces;

public interface IDashboardService
{
    /// <summary>今日工单统计</summary>
    Task<TodayOrderStatsDto> GetTodayOrderStatsAsync();

    /// <summary>工单状态分布</summary>
    Task<IEnumerable<OrderStatusDistributionDto>> GetOrderStatusDistributionAsync();

    /// <summary>产量统计</summary>
    Task<OutputStatsDto> GetOutputStatsAsync();

    /// <summary>设备状态分布</summary>
    Task<IEnumerable<EquipmentStatusDto>> GetEquipmentStatusAsync();
}
