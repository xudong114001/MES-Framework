using MES.Application.Dtos;
using MES.Application.Interfaces;
using MES.Domain.Entities;
using MES.Domain.Enums;
using MES.Domain.Repositories;

namespace MES.Application.Services;

public class DashboardService : IDashboardService
{
    private readonly IRepository<WorkOrder> _workOrderRepo;
    private readonly IRepository<Equipment> _equipmentRepo;

    public DashboardService(
        IRepository<WorkOrder> workOrderRepo,
        IRepository<Equipment> equipmentRepo)
    {
        _workOrderRepo = workOrderRepo;
        _equipmentRepo = equipmentRepo;
    }

    /// <summary>
    /// 今日工单统计
    /// </summary>
    public async Task<TodayOrderStatsDto> GetTodayOrderStatsAsync()
    {
        var today = DateTime.UtcNow.Date;
        var orders = await _workOrderRepo.FindAsync(o => o.CreatedAt >= today);
        var list = orders.ToList();
        return new TodayOrderStatsDto
        {
            Total = list.Count,
            Pending = list.Count(o => o.Status == WorkOrderStatus.PENDING),
            InProgress = list.Count(o => o.Status == WorkOrderStatus.IN_PROGRESS),
            Completed = list.Count(o => o.Status == WorkOrderStatus.COMPLETED)
        };
    }

    /// <summary>
    /// 工单状态分布
    /// </summary>
    public async Task<IEnumerable<OrderStatusDistributionDto>> GetOrderStatusDistributionAsync()
    {
        var orders = await _workOrderRepo.GetAllAsync();
        var list = orders.ToList();
        return list.GroupBy(o => o.Status)
                   .Select(g => new OrderStatusDistributionDto
                   {
                       Status = g.Key.ToString(),
                       Count = g.Count()
                   });
    }

    /// <summary>
    /// 产量统计
    /// </summary>
    public async Task<OutputStatsDto> GetOutputStatsAsync()
    {
        var today = DateTime.UtcNow.Date;
        var orders = await _workOrderRepo.FindAsync(o => o.CreatedAt >= today);
        var list = orders.ToList();
        return new OutputStatsDto
        {
            GoodQty = list.Sum(o => o.CompletedQty.Value),
            ScrapQty = list.Sum(o => o.ScrapQty.Value),
            ReworkQty = 0,
            WorkOrderCount = list.Count
        };
    }

    /// <summary>
    /// 设备状态分布
    /// </summary>
    public async Task<IEnumerable<EquipmentStatusDto>> GetEquipmentStatusAsync()
    {
        var equipment = await _equipmentRepo.GetAllAsync();
        var list = equipment.ToList();
        return list.GroupBy(e => e.Status)
                   .Select(g => new EquipmentStatusDto
                   {
                       Status = g.Key.ToString(),
                       Count = g.Count()
                   });
    }
}
