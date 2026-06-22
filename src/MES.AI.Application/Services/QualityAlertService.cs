using MES.AI.Application.Interfaces;
using MES.AI.Domain.Entities;
using MES.AI.Domain.Enums;
using MES.Domain.Entities;
using MES.Domain.Enums;
using MES.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace MES.AI.Application.Services;

public class QualityAlertService : IQualityAlertService
{
    private readonly IRepository<WorkOrder> _workOrderRepo;
    private readonly IRepository<WorkReport> _workReportRepo;
    private readonly IRepository<MaterialTrace> _materialTraceRepo;
    private readonly IRepository<AlertRecord> _alertRepo;
    private readonly ILogger<QualityAlertService>? _logger;

    private static readonly List<AlertRule> _rules =
    [
        new AlertRule
        {
            Name = "产线连续不良率飙升",
            Description = "连续3个工单报废率 > 5%",
            Condition = "consecutive_scrap_rate > 0.05",
            Level = AlertLevel.High
        },
        new AlertRule
        {
            Name = "物料批次异常",
            Description = "某批次物料在多个工单中不良率 > 3%",
            Condition = "batch_defect_rate > 0.03",
            Level = AlertLevel.Critical
        },
        new AlertRule
        {
            Name = "工位连续返工",
            Description = "某工位连续5次报工返工",
            Condition = "consecutive_rework >= 5",
            Level = AlertLevel.Medium
        }
    ];

    public QualityAlertService(
        IRepository<WorkOrder> workOrderRepo,
        IRepository<WorkReport> workReportRepo,
        IRepository<MaterialTrace> materialTraceRepo,
        IRepository<AlertRecord> alertRepo,
        ILogger<QualityAlertService>? logger = null)
    {
        _workOrderRepo = workOrderRepo;
        _workReportRepo = workReportRepo;
        _materialTraceRepo = materialTraceRepo;
        _alertRepo = alertRepo;
        _logger = logger;
    }

    public async Task<List<AlertRecord>> AnalyzeAsync(long? workOrderId = null)
    {
        var alerts = new List<AlertRecord>();
        var enabledRules = _rules.Where(r => r.IsEnabled).ToList();

        foreach (var rule in enabledRules)
        {
            try
            {
                var ruleAlerts = rule.Name switch
                {
                    "产线连续不良率飙升" => await CheckConsecutiveScrapRate(workOrderId),
                    "物料批次异常" => await CheckBatchDefectRate(workOrderId),
                    "工位连续返工" => await CheckConsecutiveRework(workOrderId),
                    _ => []
                };

                foreach (var alert in ruleAlerts)
                {
                    alert.RuleName = rule.Name;
                    alert.Level = rule.Level;
                }

                alerts.AddRange(ruleAlerts);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "规则 {RuleName} 执行异常", rule.Name);
            }
        }

        if (alerts.Count > 0)
        {
            await _alertRepo.AddRangeAsync(alerts);
            await _alertRepo.SaveChangesAsync();
        }

        return alerts;
    }

    public async Task<List<AlertRecord>> GetActiveAlertsAsync()
    {
        var result = await _alertRepo.FindAsync(a => !a.IsProcessed);
        return result.ToList();
    }

    public async Task<List<AlertRecord>> GetAlertHistoryAsync(int page = 1, int pageSize = 20)
    {
        var all = await _alertRepo.FindAsync(a => true);
        return all
            .OrderByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();
    }

    public async Task MarkAsProcessedAsync(long alertId, string processedBy)
    {
        var alert = await _alertRepo.GetByIdAsync(alertId);
        if (alert != null)
        {
            alert.IsProcessed = true;
            alert.ProcessedAt = DateTime.UtcNow;
            alert.ProcessedBy = processedBy;
            await _alertRepo.SaveChangesAsync();
        }
    }

    private async Task<List<AlertRecord>> CheckConsecutiveScrapRate(long? workOrderId)
    {
        var alerts = new List<AlertRecord>();
        var reports = await _workReportRepo.GetAllAsync();
        var orders = await _workOrderRepo.GetAllAsync();

        var orderList = orders
            .Where(o => o.Status != WorkOrderStatus.CANCELLED)
            .OrderByDescending(o => o.CreatedAt)
            .ToList();

        if (workOrderId.HasValue)
        {
            var target = orderList.FirstOrDefault(o => o.Id == workOrderId.Value);
            if (target?.LineId == null) return alerts;
            orderList = orderList.Where(o => o.LineId == target.LineId).ToList();
        }

        var lineGroups = orderList.GroupBy(o => o.LineId);

        foreach (var group in lineGroups)
        {
            var recentOrders = group.OrderByDescending(o => o.CreatedAt).Take(3).ToList();
            if (recentOrders.Count < 3) continue;

            var allHigh = true;
            foreach (var wo in recentOrders)
            {
                var woReports = reports.Where(r => r.WorkOrderId == wo.Id).ToList();
                var totalQty = woReports.Sum(r => r.GoodQty + r.ScrapQty);
                if (totalQty == 0) { allHigh = false; break; }
                var scrapRate = woReports.Sum(r => r.ScrapQty) / totalQty;
                if (scrapRate <= 0.05m) { allHigh = false; break; }
            }

            if (allHigh)
            {
                alerts.Add(new AlertRecord
                {
                    Title = "产线连续不良率飙升",
                    Message = $"产线(LineId={group.Key})连续{recentOrders.Count}个工单报废率超过5%",
                    RelatedEntityType = "ProductionLine",
                    RelatedEntityId = group.Key
                });
            }
        }

        return alerts;
    }

    private async Task<List<AlertRecord>> CheckBatchDefectRate(long? workOrderId)
    {
        var alerts = new List<AlertRecord>();
        var traces = await _materialTraceRepo.GetAllAsync();
        var reports = await _workReportRepo.GetAllAsync();

        var batchGroups = traces
            .Where(t => !string.IsNullOrEmpty(t.BatchNo) && t.WorkOrderId.HasValue)
            .GroupBy(t => t.BatchNo!);

        foreach (var batch in batchGroups)
        {
            var woIds = batch.Select(t => t.WorkOrderId!.Value).Distinct().ToList();
            if (woIds.Count < 2) continue;
            if (workOrderId.HasValue && !woIds.Contains(workOrderId.Value)) continue;

            var batchReports = reports.Where(r => woIds.Contains(r.WorkOrderId)).ToList();
            var totalQty = batchReports.Sum(r => r.GoodQty + r.ScrapQty + r.ReworkQty);
            if (totalQty == 0) continue;

            var defectQty = batchReports.Sum(r => r.ScrapQty + r.ReworkQty);
            var defectRate = defectQty / totalQty;

            if (defectRate > 0.03m)
            {
                alerts.Add(new AlertRecord
                {
                    Title = "物料批次异常",
                    Message = $"批次{batch.Key}在{woIds.Count}个工单中不良率{defectRate:P1}超过3%",
                    RelatedEntityType = "MaterialBatch",
                    RelatedEntityId = null
                });
            }
        }

        return alerts;
    }

    private async Task<List<AlertRecord>> CheckConsecutiveRework(long? workOrderId)
    {
        var alerts = new List<AlertRecord>();
        var reports = await _workReportRepo.GetAllAsync();

        var reworkReports = reports
            .Where(r => r.ReportType == ReportType.REWORK && r.WorkstationId.HasValue)
            .OrderBy(r => r.ReportTime)
            .ToList();

        var wsGroups = reworkReports.GroupBy(r => r.WorkstationId!.Value);

        foreach (var wsGroup in wsGroups)
        {
            var sorted = wsGroup.OrderBy(r => r.ReportTime).ToList();
            if (workOrderId.HasValue)
            {
                sorted = sorted.Where(r => r.WorkOrderId == workOrderId.Value).ToList();
            }

            var consecutive = 0;
            var maxConsecutive = 0;
            var lastWoId = 0L;

            foreach (var r in sorted)
            {
                if (r.WorkOrderId != lastWoId)
                {
                    consecutive = 1;
                    lastWoId = r.WorkOrderId;
                }
                else
                {
                    consecutive++;
                }

                if (consecutive > maxConsecutive)
                    maxConsecutive = consecutive;
            }

            if (maxConsecutive >= 5)
            {
                alerts.Add(new AlertRecord
                {
                    Title = "工位连续返工",
                    Message = $"工位(WorkstationId={wsGroup.Key})连续{maxConsecutive}次返工",
                    RelatedEntityType = "Workstation",
                    RelatedEntityId = wsGroup.Key
                });
            }
        }

        return alerts;
    }
}