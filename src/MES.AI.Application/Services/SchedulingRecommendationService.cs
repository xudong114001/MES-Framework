using MES.AI.Application.Dtos;
using MES.AI.Application.Interfaces;
using MES.Domain.Entities;
using MES.Domain.Enums;
using MES.Domain.Repositories;

namespace MES.AI.Application.Services;

public class SchedulingRecommendationService : ISchedulingRecommendationService
{
    private const double WeightLoad = 0.40;
    private const double WeightHistorical = 0.30;
    private const double WeightUrgency = 0.20;
    private const double WeightRouting = 0.10;
    private const int TopN = 3;

    private readonly IRepository<WorkOrder> _workOrderRepo;
    private readonly IRepository<ProductionLine> _lineRepo;
    private readonly IRepository<WorkReport> _reportRepo;
    private readonly IRepository<RoutingStep> _routingStepRepo;
    private readonly IRepository<Workstation> _workstationRepo;

    public SchedulingRecommendationService(
        IRepository<WorkOrder> workOrderRepo,
        IRepository<ProductionLine> lineRepo,
        IRepository<WorkReport> reportRepo,
        IRepository<RoutingStep> routingStepRepo,
        IRepository<Workstation> workstationRepo)
    {
        _workOrderRepo = workOrderRepo;
        _lineRepo = lineRepo;
        _reportRepo = reportRepo;
        _routingStepRepo = routingStepRepo;
        _workstationRepo = workstationRepo;
    }

    public async Task<List<ScheduleRecommendationDto>> GetRecommendationsAsync(long workOrderId)
    {
        var workOrder = await _workOrderRepo.GetByIdAsync(workOrderId)
            ?? throw new InvalidOperationException($"工单 {workOrderId} 不存在");

        var activeLines = await _lineRepo.FindAsync(l => l.Status);
        if (!activeLines.Any()) return [];

        var lineScores = new List<(ProductionLine Line, double LoadScore, double HistoricalScore,
            double UrgencyScore, double RoutingScore, double Total, double LoadRatio)>();

        foreach (var line in activeLines)
        {
            var loadScore = await CalcLoadScoreAsync(line.Id);
            var historicalScore = await CalcHistoricalScoreAsync(line.Id, workOrder.MaterialId);
            var urgencyScore = CalcUrgencyScore(workOrder);
            var routingScore = await CalcRoutingScoreAsync(line.Id, workOrder.RoutingId);

            var total = WeightLoad * loadScore
                + WeightHistorical * historicalScore
                + WeightUrgency * urgencyScore
                + WeightRouting * routingScore;

            var loadRatio = await CalcLoadRatioAsync(line.Id);

            lineScores.Add((line, loadScore, historicalScore, urgencyScore, routingScore, total, loadRatio));
        }

        var topResults = lineScores
            .OrderByDescending(x => x.Total)
            .Take(TopN)
            .ToList();

        if (!topResults.Any()) return [];

        var maxTotal = topResults.First().Total;

        var recommendations = new List<ScheduleRecommendationDto>();
        foreach (var item in topResults)
        {
            var confidence = maxTotal > 0 ? item.Total / maxTotal : 0;
            var (startTime, endTime) = await SuggestTimeWindowAsync(item.Line.Id, workOrder);

            var reasonParts = new List<string>();
            if (item.LoadScore >= 0.7) reasonParts.Add("产线负荷较低");
            else if (item.LoadScore < 0.3) reasonParts.Add("产线负荷较高");
            if (item.HistoricalScore >= 0.7) reasonParts.Add("历史完成匹配度高");
            if (item.UrgencyScore >= 0.7) reasonParts.Add("交期紧急");
            if (item.RoutingScore >= 0.7) reasonParts.Add("工艺匹配度高");
            else if (item.RoutingScore < 0.3) reasonParts.Add("工艺匹配度低");

            recommendations.Add(new ScheduleRecommendationDto
            {
                LineId = item.Line.Id,
                LineName = item.Line.Name,
                Confidence = Math.Round(confidence, 4),
                SuggestedStartTime = startTime,
                SuggestedEndTime = endTime,
                Reason = string.Join("；", reasonParts),
                LoadRatio = Math.Round(item.LoadRatio, 4),
                HistoricalMatchScore = Math.Round(item.HistoricalScore, 4)
            });
        }

        return recommendations;
    }

    private async Task<double> CalcLoadScoreAsync(long lineId)
    {
        var scheduledOrders = await _workOrderRepo.FindAsync(wo =>
            wo.LineId == lineId &&
            (wo.Status == WorkOrderStatus.SCHEDULED || wo.Status == WorkOrderStatus.IN_PROGRESS));

        var count = scheduledOrders.Count();
        double score = count switch
        {
            0 => 1.0,
            1 => 0.8,
            2 => 0.6,
            3 => 0.4,
            4 => 0.2,
            _ => 0.1
        };
        return score;
    }

    private async Task<double> CalcHistoricalScoreAsync(long lineId, long materialId)
    {
        var completedOrders = await _workOrderRepo.FindAsync(wo =>
            wo.LineId == lineId &&
            wo.MaterialId == materialId &&
            wo.Status == WorkOrderStatus.COMPLETED &&
            wo.ActualStartTime != null &&
            wo.ActualEndTime != null);

        if (!completedOrders.Any()) return 0.3;

        var onTimeCount = completedOrders.Count(wo =>
            wo.ActualEndTime <= wo.PlanEndTime);

        return (double)onTimeCount / completedOrders.Count();
    }

    private static double CalcUrgencyScore(WorkOrder workOrder)
    {
        if (workOrder.PlanEndTime == null) return 0.5;

        var now = DateTime.UtcNow;
        var planEnd = workOrder.PlanEndTime.Value;

        if (planEnd <= now) return 1.0;

        var planStart = workOrder.PlanStartTime ?? now;
        var totalPeriod = (planEnd - planStart).TotalMinutes;
        if (totalPeriod <= 0) return 1.0;

        var remaining = (planEnd - now).TotalMinutes;
        var ratio = remaining / totalPeriod;

        return ratio switch
        {
            <= 0.2 => 1.0,
            <= 0.4 => 0.8,
            <= 0.6 => 0.6,
            <= 0.8 => 0.4,
            _ => 0.2
        };
    }

    private async Task<double> CalcRoutingScoreAsync(long lineId, long? routingId)
    {
        if (routingId == null) return 0.5;

        var routingSteps = await _routingStepRepo.FindAsync(rs => rs.RoutingId == routingId.Value);
        if (!routingSteps.Any()) return 0.3;

        var workstations = await _workstationRepo.FindAsync(ws => ws.LineId == lineId);
        if (!workstations.Any()) return 0.1;

        var workstationCount = workstations.Count();
        var stepCount = routingSteps.Count();

        if (stepCount <= workstationCount) return 1.0;
        if (stepCount <= workstationCount * 2) return 0.7;
        return 0.3;
    }

    private async Task<double> CalcLoadRatioAsync(long lineId)
    {
        var scheduledOrders = await _workOrderRepo.FindAsync(wo =>
            wo.LineId == lineId &&
            (wo.Status == WorkOrderStatus.SCHEDULED || wo.Status == WorkOrderStatus.IN_PROGRESS));

        var totalPlanned = scheduledOrders.Sum(wo => wo.PlannedQty);
        var totalCompleted = scheduledOrders.Sum(wo => wo.CompletedQty);

        if (totalPlanned == 0) return 0;
        return (double)totalCompleted / (double)totalPlanned;
    }

    private async Task<(DateTime Start, DateTime End)> SuggestTimeWindowAsync(long lineId, WorkOrder workOrder)
    {
        var now = DateTime.UtcNow;
        var scheduledOrders = (await _workOrderRepo.FindAsync(wo =>
            wo.LineId == lineId &&
            (wo.Status == WorkOrderStatus.SCHEDULED || wo.Status == WorkOrderStatus.IN_PROGRESS) &&
            wo.PlanEndTime != null))
            .OrderByDescending(wo => wo.PlanEndTime)
            .ToList();

        var earliestStart = scheduledOrders.Any()
            ? scheduledOrders.Max(wo => wo.PlanEndTime!.Value)
            : now;

        if (earliestStart < now) earliestStart = now;

        var duration = EstimateDurationAsync(workOrder);
        var suggestedEnd = earliestStart.AddMinutes(duration);

        return (earliestStart, suggestedEnd);
    }

    private static double EstimateDurationAsync(WorkOrder workOrder)
    {
        if (workOrder.PlanStartTime.HasValue && workOrder.PlanEndTime.HasValue)
        {
            var planned = (workOrder.PlanEndTime.Value - workOrder.PlanStartTime.Value).TotalMinutes;
            if (planned > 0) return planned;
        }

        return (double)workOrder.PlannedQty * 10;
    }
}
