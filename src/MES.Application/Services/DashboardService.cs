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
    public async Task<object> GetTodayOrderStatsAsync()
    {
        var today = DateTime.UtcNow.Date;
        var orders = await _workOrderRepo.FindAsync(o => o.CreatedAt >= today);
        var list = orders.ToList();
        return new
        {
            total = list.Count,
            pending = list.Count(o => o.Status == WorkOrderStatus.PENDING),
            inProgress = list.Count(o => o.Status == WorkOrderStatus.IN_PROGRESS),
            completed = list.Count(o => o.Status == WorkOrderStatus.COMPLETED),
            cancelled = list.Count(o => o.Status == WorkOrderStatus.CANCELLED)
        };
    }

    /// <summary>
    /// 工单状态分布
    /// </summary>
    public async Task<object> GetOrderStatusDistributionAsync()
    {
        var orders = await _workOrderRepo.GetAllAsync();
        var list = orders.ToList();
        return list.GroupBy(o => o.Status)
                   .Select(g => new { status = g.Key.ToString(), count = g.Count() });
    }

    /// <summary>
    /// 产量统计
    /// </summary>
    public async Task<object> GetOutputStatsAsync()
    {
        var today = DateTime.UtcNow.Date;
        var orders = await _workOrderRepo.FindAsync(o => o.CreatedAt >= today);
        var list = orders.ToList();
        return new
        {
            plannedQty = list.Sum(o => o.PlannedQty),
            completedQty = list.Sum(o => o.CompletedQty),
            scrapQty = list.Sum(o => o.ScrapQty)
        };
    }

    /// <summary>
    /// 设备状态分布
    /// </summary>
    public async Task<object> GetEquipmentStatusAsync()
    {
        var equipment = await _equipmentRepo.GetAllAsync();
        var list = equipment.ToList();
        return list.GroupBy(e => e.Status)
                   .Select(g => new { status = g.Key.ToString(), count = g.Count() });
    }
}
