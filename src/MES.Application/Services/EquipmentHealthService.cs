using MES.Application.Dtos;
using MES.Application.Interfaces;
using MES.Domain.Entities;
using MES.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace MES.Application.Services;

public class EquipmentHealthService : IEquipmentHealthService
{
    private readonly IRepository<Equipment> _equipmentRepo;
    private readonly IRepository<WorkReport> _workReportRepo;
    private readonly IRepository<WorkOrder> _workOrderRepo;

    public EquipmentHealthService(
        IRepository<Equipment> equipmentRepo,
        IRepository<WorkReport> workReportRepo,
        IRepository<WorkOrder> workOrderRepo)
    {
        _equipmentRepo = equipmentRepo;
        _workReportRepo = workReportRepo;
        _workOrderRepo = workOrderRepo;
    }

    public async Task<EquipmentHealthDto> AnalyzeEquipmentAsync(long equipmentId)
    {
        var equipment = await _equipmentRepo.GetByIdAsync(equipmentId);
        if (equipment == null)
            throw new InvalidOperationException("设备不存在");

        var dailyOee = await CalculateDailyOeeAsync(equipmentId, 30);

        var oee7DayTrend = CalculateLinearRegressionSlope(dailyOee.TakeLast(7).ToList());
        var oee30DayTrend = CalculateLinearRegressionSlope(dailyOee);

        var healthScore = CalculateHealthScore(dailyOee, oee7DayTrend, oee30DayTrend, equipment);

        var riskLevel = GetRiskLevel(healthScore);
        var predictedDate = PredictMaintenanceDate(healthScore, oee30DayTrend, equipment);
        var recommendation = GetRecommendation(healthScore, oee7DayTrend, oee30DayTrend);

        return new EquipmentHealthDto
        {
            EquipmentId = equipmentId,
            EquipmentName = equipment.Name,
            HealthScore = healthScore,
            RiskLevel = riskLevel,
            Oee7DayTrend = Math.Round(oee7DayTrend, 4),
            Oee30DayTrend = Math.Round(oee30DayTrend, 4),
            PredictedMaintenanceDate = predictedDate,
            Recommendation = recommendation
        };
    }

    public async Task<List<EquipmentHealthDto>> GetAllEquipmentHealthAsync()
    {
        var equipments = await _equipmentRepo.GetAllAsync();
        var results = new List<EquipmentHealthDto>();

        foreach (var eq in equipments)
        {
            try
            {
                var health = await AnalyzeEquipmentAsync(eq.Id);
                results.Add(health);
            }
            catch
            {
                results.Add(new EquipmentHealthDto
                {
                    EquipmentId = eq.Id,
                    EquipmentName = eq.Name,
                    HealthScore = 0,
                    RiskLevel = "未知",
                    Recommendation = "无法分析设备数据"
                });
            }
        }

        return results;
    }

    public async Task<List<EquipmentHealthDto>> GetHighRiskEquipmentAsync()
    {
        var all = await GetAllEquipmentHealthAsync();
        return all.Where(h => h.HealthScore < 70).ToList();
    }

    private async Task<List<DailyOeePoint>> CalculateDailyOeeAsync(long equipmentId, int days)
    {
        var since = DateTime.UtcNow.AddDays(-days);
        var reports = await _workReportRepo
            .FindAsync(r => r.ReportTime >= since && !r.WorkOrder!.IsDeleted)
            .ContinueWith(t => t.Result.ToList());

        var equipment = await _equipmentRepo.GetByIdAsync(equipmentId);

        var grouped = reports
            .GroupBy(r => r.ReportTime.Date)
            .OrderBy(g => g.Key)
            .ToList();

        var points = new List<DailyOeePoint>();

        foreach (var group in grouped)
        {
            var dayReports = group.ToList();
            var goodQty = (double)dayReports.Sum(r => r.GoodQty.Value);
            var badQty = (double)dayReports.Sum(r => (r.ScrapQty + r.ReworkQty).Value);
            var actualRunMinutes = dayReports.Sum(r => r.DurationMin);

            double plannedRunMinutes = equipment?.PlannedRunTime.HasValue == true && equipment.PlannedRunTime.Value > 0
                ? equipment.PlannedRunTime.Value * 60
                : Math.Max(actualRunMinutes, 1);

            var availability = Math.Min(actualRunMinutes / plannedRunMinutes, 1.0);
            var totalQty = goodQty + badQty;
            var quality = totalQty > 0 ? goodQty / totalQty : 0;

            double performance = 1.0;
            if (equipment?.TheoreticalCycleTime.HasValue == true && equipment.TheoreticalCycleTime.Value > 0 && actualRunMinutes > 0)
            {
                var theoreticalOutput = (actualRunMinutes * 60) / equipment.TheoreticalCycleTime.Value;
                performance = totalQty > 0 ? Math.Min(theoreticalOutput / totalQty, 1.0) : 0;
            }

            var oee = availability * performance * quality;

            points.Add(new DailyOeePoint
            {
                Date = group.Key,
                OeeValue = oee
            });
        }

        return points;
    }

    private static double CalculateLinearRegressionSlope(List<DailyOeePoint> points)
    {
        if (points.Count < 2) return 0;

        var n = points.Count;
        double sumX = 0, sumY = 0, sumXY = 0, sumX2 = 0;

        for (var i = 0; i < n; i++)
        {
            var x = (double)i;
            var y = points[i].OeeValue;
            sumX += x;
            sumY += y;
            sumXY += x * y;
            sumX2 += x * x;
        }

        var denominator = n * sumX2 - sumX * sumX;
        if (Math.Abs(denominator) < 1e-10) return 0;

        return (n * sumXY - sumX * sumY) / denominator;
    }

    private static int CalculateHealthScore(
        List<DailyOeePoint> dailyOee,
        double oee7DayTrend,
        double oee30DayTrend,
        Equipment equipment)
    {
        if (dailyOee.Count == 0) return 50;

        var recentOee = dailyOee.TakeLast(7).Average(p => p.OeeValue);
        var overallOee = dailyOee.Average(p => p.OeeValue);

        var baseScore = recentOee * 60;

        var trendPenalty = 0.0;
        if (oee7DayTrend < 0) trendPenalty += Math.Abs(oee7DayTrend) * 200;
        if (oee30DayTrend < 0) trendPenalty += Math.Abs(oee30DayTrend) * 100;

        var maintenanceBonus = 0.0;
        if (equipment.LastMaintainDate.HasValue)
        {
            var daysSinceMaintenance = (DateTime.UtcNow - equipment.LastMaintainDate.Value).TotalDays;
            if (daysSinceMaintenance < 7) maintenanceBonus = 5;
            else if (daysSinceMaintenance > 60) maintenanceBonus = -10;
        }
        else
        {
            maintenanceBonus = -10;
        }

        if (equipment.NextMaintainDate.HasValue && equipment.NextMaintainDate.Value < DateTime.UtcNow)
            maintenanceBonus -= 15;

        var rawScore = baseScore - trendPenalty + maintenanceBonus;

        return (int)Math.Clamp(Math.Round(rawScore), 0, 100);
    }

    private static string GetRiskLevel(int healthScore) => healthScore switch
    {
        >= 90 => "优",
        >= 70 => "良",
        >= 50 => "预警",
        _ => "危险"
    };

    private static DateTime? PredictMaintenanceDate(int healthScore, double oee30DayTrend, Equipment equipment)
    {
        if (equipment.NextMaintainDate.HasValue)
        {
            if (equipment.NextMaintainDate.Value < DateTime.UtcNow)
                return DateTime.UtcNow.AddDays(1);

            return equipment.NextMaintainDate;
        }

        if (healthScore >= 90) return null;

        if (oee30DayTrend < -0.005)
        {
            var currentOee = healthScore / 100.0;
            var daysToThreshold = oee30DayTrend != 0
                ? (0.5 - currentOee) / oee30DayTrend
                : 30;
            daysToThreshold = Math.Max(1, Math.Min(daysToThreshold, 30));
            return DateTime.UtcNow.AddDays(daysToThreshold);
        }

        if (healthScore < 70)
            return DateTime.UtcNow.AddDays(7);

        return DateTime.UtcNow.AddDays(14);
    }

    private static string GetRecommendation(int healthScore, double oee7DayTrend, double oee30DayTrend)
    {
        return healthScore switch
        {
            >= 90 => "设备运行正常，继续保持常规监控",
            >= 70 when oee7DayTrend < 0 => "设备OEE呈下降趋势，建议关注运行状态并增加巡检频次",
            >= 70 => "设备状态良好，建议持续关注趋势变化",
            >= 50 when oee30DayTrend < -0.01 => "设备性能持续下降，建议尽快安排预防性维护",
            >= 50 => "设备处于预警状态，建议在近期安排维护保养",
            _ when oee7DayTrend < -0.02 => "设备严重劣化且快速下降，建议立即停机维护",
            _ => "设备健康状态危险，建议立即安排维护"
        };
    }

    private class DailyOeePoint
    {
        public DateTime Date { get; set; }
        public double OeeValue { get; set; }
    }
}
