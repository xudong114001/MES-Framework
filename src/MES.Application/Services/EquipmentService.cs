using MES.Domain.Entities;
using MES.Domain.Enums;
using MES.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace MES.Application.Services;

public class EquipmentService
{
    private readonly IRepository<Equipment> _equipmentRepo;
    private readonly IRepository<WorkReport> _workReportRepo;
    private readonly IRepository<WorkOrder> _workOrderRepo;
    private readonly IRepository<MaintenancePlan> _maintenancePlanRepo;

    public EquipmentService(
        IRepository<Equipment> equipmentRepo,
        IRepository<WorkReport> workReportRepo,
        IRepository<WorkOrder> workOrderRepo,
        IRepository<MaintenancePlan> maintenancePlanRepo)
    {
        _equipmentRepo = equipmentRepo;
        _workReportRepo = workReportRepo;
        _workOrderRepo = workOrderRepo;
        _maintenancePlanRepo = maintenancePlanRepo;
    }

    // 记录保养
    public async Task RecordMaintenanceAsync(long equipmentId)
    {
        var eq = await _equipmentRepo.GetByIdAsync(equipmentId);
        if (eq == null) throw new InvalidOperationException("设备不存在");
        eq.LastMaintainDate = DateTime.UtcNow;
        if (eq.MaintainCycle > 0)
            eq.NextMaintainDate = DateTime.UtcNow.AddDays((double)eq.MaintainCycle);
        eq.Status = EquipmentStatus.RUNNING;
        await _equipmentRepo.UpdateAsync(eq);
    }

    // 报修
    public async Task ReportFaultAsync(long equipmentId)
    {
        var eq = await _equipmentRepo.GetByIdAsync(equipmentId);
        if (eq == null) throw new InvalidOperationException("设备不存在");
        eq.Status = EquipmentStatus.BROKEN;
        await _equipmentRepo.UpdateAsync(eq);
    }

    /// <summary>
    /// 真实 OEE 计算
    /// OEE = 时间开动率 × 性能开动率 × 良品率
    /// </summary>
    public async Task<OeeResult> CalculateOeeAsync(long equipmentId)
    {
        var eq = await _equipmentRepo.GetByIdAsync(equipmentId);
        if (eq == null) throw new InvalidOperationException("设备不存在");

        // 从 WorkReport 统计今日报工数据（最近30天有效数据）
        var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
        var reports = await _workReportRepo
            .FindAsync(r =>
                r.WorkOrder != null &&
                r.ReportTime >= thirtyDaysAgo &&
                !r.WorkOrder.IsDeleted)
            .ContinueWith(t => t.Result.ToList());

        // 如果没有报工数据，返回默认值
        if (reports.Count == 0)
        {
            return new OeeResult
            {
                EquipmentId = eq.Id,
                EquipmentName = eq.Name,
                Status = eq.Status.ToString(),
                OeeValue = 0,
                Availability = 0,
                Performance = 0,
                Quality = 0,
                GoodQty = 0,
                BadQty = 0,
                ActualRunMinutes = 0,
                PlannedRunMinutes = 0,
                LastMaintainTime = eq.LastMaintainDate,
                NextMaintainTime = eq.NextMaintainDate,
                MaintainCycleDays = eq.MaintainCycle
            };
        }

        // 良品/不良品统计
        var goodQty = (double)reports.Sum(r => (decimal)r.GoodQty);
        var badQty = (double)reports.Sum(r => (decimal)(r.ScrapQty + r.ReworkQty));

        // 实际运行时间（从报工时长累加，单位：分钟）
        var actualRunMinutes = reports.Sum(r => r.DurationMin);

        // 计划运行时间
        double plannedRunMinutes;
        if (eq.PlannedRunTime.HasValue && eq.PlannedRunTime.Value > 0)
        {
            // 按日计划运行时间估算（取30天内的天数）
            var coverageDays = (int)Math.Max(1, Math.Ceiling(
                (reports.Max(r => r.ReportTime) - reports.Min(r => r.ReportTime)).TotalDays));
            plannedRunMinutes = eq.PlannedRunTime.Value * 60 * coverageDays;
        }
        else
        {
            // 从工单的计划时间计算
            var workOrderIds = reports.Select(r => r.WorkOrderId).Distinct().ToList();
            var workOrders = await _workOrderRepo
                .FindAsync(wo => workOrderIds.Contains(wo.Id))
                .ContinueWith(t => t.Result.ToList());

            plannedRunMinutes = workOrders
                .Where(wo => wo.PlanStartTime.HasValue && wo.PlanEndTime.HasValue)
                .Sum(wo => (wo.PlanEndTime!.Value - wo.PlanStartTime!.Value).TotalMinutes);
        }

        if (plannedRunMinutes <= 0) plannedRunMinutes = actualRunMinutes > 0 ? actualRunMinutes : 1;

        // 计算时间开动率
        var availability = actualRunMinutes / plannedRunMinutes;
        if (availability > 1) availability = 1; // 实际运行时间不可能超过计划

        // 计算性能开动率
        double performance;
        if (eq.TheoreticalCycleTime.HasValue && eq.TheoreticalCycleTime.Value > 0 && actualRunMinutes > 0)
        {
            // 理论节拍（秒/件）→ 理论产出 = 实际运行时间(秒) / 理论节拍
            var theoreticalOutput = (actualRunMinutes * 60) / eq.TheoreticalCycleTime.Value;
            var actualTotal = goodQty + badQty;
            performance = actualTotal > 0 ? theoreticalOutput / actualTotal : 0;
            // 性能开动率通常不应超过 1 (100%)，但理论偏差可接受
            if (performance > 1) performance = 1;
        }
        else
        {
            // 没有理论节拍时，假设性能开动率为 1
            performance = 1;
        }

        // 计算良品率
        var totalQty = goodQty + badQty;
        var quality = totalQty > 0 ? goodQty / totalQty : 0;

        // 计算 OEE
        var oeeValue = availability * performance * quality;

        return new OeeResult
        {
            EquipmentId = eq.Id,
            EquipmentName = eq.Name,
            Status = eq.Status.ToString(),
            OeeValue = Math.Round(oeeValue, 4),
            Availability = Math.Round(availability, 4),
            Performance = Math.Round(performance, 4),
            Quality = Math.Round(quality, 4),
            GoodQty = goodQty,
            BadQty = badQty,
            ActualRunMinutes = actualRunMinutes,
            PlannedRunMinutes = plannedRunMinutes,
            LastMaintainTime = eq.LastMaintainDate,
            NextMaintainTime = eq.NextMaintainDate,
            MaintainCycleDays = eq.MaintainCycle,
            TheoreticalCycleTime = eq.TheoreticalCycleTime,
            PlannedRunTime = eq.PlannedRunTime
        };
    }

    // ======================== 保养计划管理 ========================

    public async Task<MaintenancePlan> CreateMaintenancePlanAsync(
        long equipmentId, string planName, int cycleDays, string? description)
    {
        var eq = await _equipmentRepo.GetByIdAsync(equipmentId);
        if (eq == null) throw new InvalidOperationException("设备不存在");

        var plan = new MaintenancePlan
        {
            EquipmentId = equipmentId,
            PlanName = planName,
            CycleDays = cycleDays,
            NextDueDate = DateTime.UtcNow.AddDays(cycleDays),
            Description = description,
            Status = MaintenancePlanStatus.PENDING
        };

        return await _maintenancePlanRepo.AddAsync(plan);
    }

    public async Task<List<MaintenancePlan>> GetMaintenancePlansAsync(long equipmentId)
    {
        var plans = await _maintenancePlanRepo
            .FindAsync(p => p.EquipmentId == equipmentId)
            .ContinueWith(t => t.Result.ToList());
        return plans;
    }

    public async Task CompleteMaintenanceAsync(long planId)
    {
        var plan = await _maintenancePlanRepo.GetByIdAsync(planId);
        if (plan == null) throw new InvalidOperationException("保养计划不存在");

        plan.LastCompletedDate = DateTime.UtcNow;
        plan.NextDueDate = DateTime.UtcNow.AddDays(plan.CycleDays);
        plan.Status = MaintenancePlanStatus.COMPLETED;
        await _maintenancePlanRepo.UpdateAsync(plan);

        // 同时更新设备的保养记录
        var eq = await _equipmentRepo.GetByIdAsync(plan.EquipmentId);
        if (eq != null)
        {
            eq.LastMaintainDate = DateTime.UtcNow;
            eq.NextMaintainDate = plan.NextDueDate;
            eq.Status = EquipmentStatus.RUNNING;
            await _equipmentRepo.UpdateAsync(eq);
        }
    }
}

public class OeeResult
{
    public long EquipmentId { get; set; }
    public string EquipmentName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public double OeeValue { get; set; }
    public double Availability { get; set; }
    public double Performance { get; set; }
    public double Quality { get; set; }
    public double GoodQty { get; set; }
    public double BadQty { get; set; }
    public double ActualRunMinutes { get; set; }
    public double PlannedRunMinutes { get; set; }
    public DateTime? LastMaintainTime { get; set; }
    public DateTime? NextMaintainTime { get; set; }
    public int? MaintainCycleDays { get; set; }
    public double? TheoreticalCycleTime { get; set; }
    public double? PlannedRunTime { get; set; }
}
