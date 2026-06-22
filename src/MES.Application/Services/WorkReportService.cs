using Microsoft.Extensions.Logging;
using MES.Application.Dtos;
using MES.Application.Interfaces;
using MES.Application.Integration.Events;
using MES.Domain.Entities;
using MES.Domain.Enums;
using MES.Infrastructure.Repositories;
using StackExchange.Redis;

namespace MES.Application.Services;

public class WorkReportService : IWorkReportService
{
    private readonly IRepository<WorkReport> _reportRepo;
    private readonly IRepository<WorkOrder> _workOrderRepo;
    private readonly IRepository<WorkOrderStep> _stepRepo;
    private readonly IRepository<Workstation> _workstationRepo;
    private readonly IRepository<User> _userRepo;
    private readonly IRepository<QcCheckpoint> _checkpointRepo;
    private readonly IRepository<QcInspection> _inspectionRepo;
    private readonly IDatabase _redis;
    private readonly IEventBus? _eventBus;
    private readonly InMemoryEventLogService? _eventLog;
    private readonly ILogger<WorkReportService>? _logger;

    public WorkReportService(
        IRepository<WorkReport> reportRepo,
        IRepository<WorkOrder> workOrderRepo,
        IRepository<WorkOrderStep> stepRepo,
        IRepository<Workstation> workstationRepo,
        IRepository<User> userRepo,
        IRepository<QcCheckpoint> checkpointRepo,
        IRepository<QcInspection> inspectionRepo,
        IConnectionMultiplexer redisConn,
        IEventBus? eventBus = null,
        InMemoryEventLogService? eventLog = null,
        ILogger<WorkReportService>? logger = null)
    {
        _reportRepo = reportRepo;
        _workOrderRepo = workOrderRepo;
        _stepRepo = stepRepo;
        _workstationRepo = workstationRepo;
        _userRepo = userRepo;
        _checkpointRepo = checkpointRepo;
        _inspectionRepo = inspectionRepo;
        _redis = redisConn.GetDatabase();
        _eventBus = eventBus;
        _eventLog = eventLog;
        _logger = logger;
    }

    /// <summary>
    /// 将 WorkReport 实体映射为 DTO
    /// </summary>
    private static WorkReportDto MapToDto(WorkReport entity)
    {
        return new WorkReportDto
        {
            Id = entity.Id,
            ReportNo = entity.ReportNo,
            WorkOrderId = entity.WorkOrderId,
            StepId = entity.StepId,
            WorkstationId = entity.WorkstationId,
            OperatorId = entity.OperatorId,
            ReportType = entity.ReportType,
            GoodQty = entity.GoodQty,
            ScrapQty = entity.ScrapQty,
            ReworkQty = entity.ReworkQty,
            DurationMin = entity.DurationMin,
            ReportTime = entity.ReportTime,
            Remark = entity.Remark,
            BatchNo = entity.BatchNo,
            CreatedAt = entity.CreatedAt,
            CreatedBy = entity.CreatedBy,
            UpdatedAt = entity.UpdatedAt,
            UpdatedBy = entity.UpdatedBy
        };
    }

    /// <summary>
    /// 获取所有报工记录
    /// </summary>
    public async Task<IEnumerable<WorkReportDto>> GetAllAsync()
    {
        var entities = await _reportRepo.GetAllAsync();
        return entities.Select(MapToDto);
    }

    /// <summary>
    /// 根据ID获取报工记录
    /// </summary>
    public async Task<WorkReportDto?> GetByIdAsync(long id)
    {
        var entity = await _reportRepo.GetByIdAsync(id);
        return entity == null ? null : MapToDto(entity);
    }

    /// <summary>
    /// 更新报工记录
    /// </summary>
    public async Task UpdateWorkReportAsync(WorkReport report)
    {
        var existing = await _reportRepo.GetByIdAsync(report.Id);
        if (existing == null)
            throw new InvalidOperationException("报工记录不存在");

        // 保留审计字段
        report.CreatedAt = existing.CreatedAt;
        report.CreatedBy = existing.CreatedBy;
        report.UpdatedAt = DateTime.UtcNow;

        await _reportRepo.UpdateAsync(report);
    }

    /// <summary>
    /// 提交报工（含 Redis 防重复提交 + 批次号自动生成 + 质检校验）
    /// </summary>
    public async Task<WorkReport> SubmitReportAsync(WorkReport report)
    {
        // ==================== Redis 防重复提交 ====================
        var dedupKey = $"report:dup:{report.WorkOrderId}:{report.StepId}:{report.OperatorId}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds() / 5}";
        var locked = await _redis.StringSetAsync(dedupKey, "1", TimeSpan.FromSeconds(10), When.NotExists);
        if (!locked)
            throw new InvalidOperationException("请勿重复提交");

        // ==================== 质检校验 ====================
        if (report.StepId.HasValue)
        {
            await ValidateQcCheckpointAsync(report.WorkOrderId, report.StepId.Value);
        }

        // ==================== 批次号自动生成 ====================
        if (report.GoodQty > 0)
        {
            var today = DateTime.Now.ToString("yyyyMMdd");
            var seqKey = $"batch:seq:{today}";
            var seq = await _redis.StringIncrementAsync(seqKey);
            // 设置 key 有效期到第二天，避免 key 永久存在
            if (seq == 1)
                await _redis.KeyExpireAsync(seqKey, TimeSpan.FromDays(2));
            report.BatchNo = $"BAT{today}-{seq:D4}";
        }

        // ==================== 原有报工校验和更新逻辑 ====================
        var wo = await _workOrderRepo.GetByIdAsync(report.WorkOrderId);
        if (wo == null) throw new InvalidOperationException("工单不存在");
        if (wo.Status != WorkOrderStatus.RELEASED && wo.Status != WorkOrderStatus.IN_PROGRESS)
            throw new InvalidOperationException($"工单状态({wo.Status})不允许报工");

        // 校验报工数量不超过计划
        var totalReported = wo.CompletedQty + wo.ScrapQty;
        var currentTotal = report.GoodQty + report.ScrapQty + report.ReworkQty;
        if (totalReported + currentTotal > wo.PlannedQty)
            throw new InvalidOperationException($"报工数量({currentTotal})超过剩余可报工数量({wo.PlannedQty - totalReported})");

        // 使用领域方法更新工单进度
        wo.ReportProgress(report.GoodQty, report.ScrapQty, report.ReworkQty);

        await _workOrderRepo.UpdateAsync(wo);

        // 更新对应工序进度
        if (report.StepId.HasValue)
        {
            var step = (await _stepRepo.FindAsync(s => s.Id == report.StepId.Value)).FirstOrDefault();
            if (step != null)
            {
                step.UpdateProgress(report.GoodQty, report.ScrapQty);
                await _stepRepo.UpdateAsync(step);
            }
        }

        var created = await _reportRepo.AddAsync(report);

        // 发布报工提交事件（失败不影响主事务）
        try
        {
            if (_eventBus != null)
            {
                var evt = new WorkReportSubmittedEvent
                {
                    WorkReportId = created.Id,
                    ReportNo = created.ReportNo,
                    WorkOrderId = created.WorkOrderId,
                    GoodQty = created.GoodQty,
                    ScrapQty = created.ScrapQty,
                    ReworkQty = created.ReworkQty,
                    ReportType = created.ReportType,
                    BatchNo = created.BatchNo
                };
                await _eventBus.Publish(evt);
                _eventLog?.Log(evt, "Published");
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to publish WorkReportSubmittedEvent");
            _eventLog?.Log(new WorkReportSubmittedEvent
            {
                WorkReportId = created.Id,
                ReportNo = created.ReportNo,
                WorkOrderId = created.WorkOrderId,
                GoodQty = created.GoodQty,
                ScrapQty = created.ScrapQty,
                ReworkQty = created.ReworkQty,
                ReportType = created.ReportType
            }, "Failed", ex.Message);
        }

        return created;
    }

    /// <summary>
    /// 质检校验：检查该工序是否有强制质检点且完成质检
    /// </summary>
    private async Task ValidateQcCheckpointAsync(long workOrderId, long stepId)
    {
        // 查找该工序所有的强制质检点
        var mandatoryCheckpoints = await _checkpointRepo.FindAsync(
            c => c.StepId == stepId && c.IsMandatory);

        if (!mandatoryCheckpoints.Any())
            return; // 没有强制质检点，无需检查

        // 对于每个强制质检点，检查是否有关联的质检单已完成
        foreach (var cp in mandatoryCheckpoints)
        {
            var pendingInspections = await _inspectionRepo.FindAsync(
                i => i.WorkOrderId == workOrderId
                     && i.SourceType == cp.CheckType
                     && i.InspectResult == QcResult.PENDING);

            if (pendingInspections.Any())
            {
                throw new InvalidOperationException("该工序需先完成质检");
            }
        }
    }

    /// <summary>
    /// PDA 扫码报工
    /// </summary>
    public async Task<WorkReport> PdaScanReportAsync(PdaScanReportRequest request)
    {
        // 1. 根据 scan_code 查找工单
        var orders = await _workOrderRepo.FindAsync(o => o.OrderNo == request.ScanCode);
        var wo = orders.FirstOrDefault();
        if (wo == null)
            throw new InvalidOperationException($"未找到工单: {request.ScanCode}");

        // 2. 根据 workstation_code 查找工位
        var workstations = await _workstationRepo.FindAsync(ws => ws.Code == request.WorkstationCode);
        var workstation = workstations.FirstOrDefault();
        if (workstation == null)
            throw new InvalidOperationException($"未找到工位: {request.WorkstationCode}");

        // 3. 根据 operator_code 查找操作工
        var operators = await _userRepo.FindAsync(u => u.Username == request.OperatorCode);
        var operatorUser = operators.FirstOrDefault();
        if (operatorUser == null)
            throw new InvalidOperationException($"未找到操作工: {request.OperatorCode}");

        // 4. 根据工单ID + 工序名称 匹�� WorkOrderStep
        WorkOrderStep? matchedStep = null;
        var steps = await _stepRepo.FindAsync(s => s.WorkOrderId == wo.Id && s.StepName == request.StepName);
        matchedStep = steps.FirstOrDefault();

        // 5. 构建 WorkReport 并提交
        var report = WorkReport.Create(
            workOrderId: wo.Id,
            reportType: ReportType.COMPLETE,
            goodQty: request.GoodQty,
            scrapQty: request.ScrapQty,
            reworkQty: request.ReworkQty,
            stepId: matchedStep?.Id,
            workstationId: workstation.Id,
            operatorId: operatorUser.Id,
            remark: $"PDA扫码报工 - {request.WorkstationCode}"
        );

        return await SubmitReportAsync(report);
    }
}
