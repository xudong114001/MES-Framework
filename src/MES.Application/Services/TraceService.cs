using MES.Application.Dtos;
using MES.Application.Interfaces;
using MES.Domain.Entities;
using MES.Domain.Repositories;

namespace MES.Application.Services;

public class TraceService : ITraceService
{
    private readonly IRepository<MaterialTrace> _traceRepo;
    private readonly IRepository<Material> _materialRepo;
    private readonly IRepository<WorkOrder> _workOrderRepo;

    public TraceService(
        IRepository<MaterialTrace> traceRepo,
        IRepository<Material> materialRepo,
        IRepository<WorkOrder> workOrderRepo)
    {
        _traceRepo = traceRepo;
        _materialRepo = materialRepo;
        _workOrderRepo = workOrderRepo;
    }

    // 按批次查询
    public async Task<TraceResultDto> TraceByBatchAsync(string batchNo)
    {
        var traces = await _traceRepo.FindAsync(t => t.BatchNo == batchNo);
        var list = traces.ToList();

        var material = list.FirstOrDefault() != null
            ? await _materialRepo.GetByIdAsync(list.First().MaterialId)
            : null;

        return new TraceResultDto
        {
            TraceType = "Batch",
            BatchNo = batchNo,
            MaterialCode = material?.Code ?? string.Empty,
            MaterialName = material?.Name ?? string.Empty,
            Steps = list.Select(t => new TraceStepDto
            {
                WorkOrderId = t.WorkOrderId ?? 0,
                Direction = t.Direction ?? string.Empty,
                Qty = t.Qty,
                OperateTime = t.OperateTime
            }).ToList()
        };
    }

    // 按序列号查询
    public async Task<TraceResultDto> TraceBySerialAsync(string serialNo)
    {
        var traces = await _traceRepo.FindAsync(t => t.SerialNo == serialNo);
        var list = traces.ToList();

        var material = list.FirstOrDefault() != null
            ? await _materialRepo.GetByIdAsync(list.First().MaterialId)
            : null;

        return new TraceResultDto
        {
            TraceType = "Serial",
            SerialNo = serialNo,
            BatchNo = list.FirstOrDefault()?.BatchNo ?? string.Empty,
            MaterialCode = material?.Code ?? string.Empty,
            MaterialName = material?.Name ?? string.Empty,
            Steps = list.Select(t => new TraceStepDto
            {
                WorkOrderId = t.WorkOrderId ?? 0,
                Direction = t.Direction ?? string.Empty,
                Qty = t.Qty,
                OperateTime = t.OperateTime
            }).ToList()
        };
    }

    // 正向追溯：原料批次 → 消耗到哪些工单 → 产出哪些成品
    public async Task<TraceResultDto> TraceForwardAsync(long materialId, string batchNo)
    {
        var traces = await _traceRepo.FindAsync(t => t.MaterialId == materialId && t.BatchNo == batchNo && t.Direction == "CONSUME");
        var list = traces.ToList();

        var material = await _materialRepo.GetByIdAsync(materialId);

        return new TraceResultDto
        {
            TraceType = "Forward",
            BatchNo = batchNo,
            MaterialCode = material?.Code ?? string.Empty,
            MaterialName = material?.Name ?? string.Empty,
            Steps = list.Select(t => new TraceStepDto
            {
                WorkOrderId = t.WorkOrderId ?? 0,
                Direction = t.Direction ?? string.Empty,
                Qty = t.Qty,
                OperateTime = t.OperateTime
            }).ToList()
        };
    }

    // 反向追溯：成品序列号 → 用了哪些原料批次
    public async Task<TraceResultDto> TraceBackwardAsync(string serialNo)
    {
        var traces = await _traceRepo.FindAsync(t => t.SerialNo == serialNo);
        var list = traces.ToList();

        var material = list.FirstOrDefault() != null
            ? await _materialRepo.GetByIdAsync(list.First().MaterialId)
            : null;

        return new TraceResultDto
        {
            TraceType = "Backward",
            SerialNo = serialNo,
            BatchNo = list.FirstOrDefault()?.BatchNo ?? string.Empty,
            MaterialCode = material?.Code ?? string.Empty,
            MaterialName = material?.Name ?? string.Empty,
            Steps = list.Select(t => new TraceStepDto
            {
                WorkOrderId = t.WorkOrderId ?? 0,
                Direction = t.Direction ?? string.Empty,
                Qty = t.Qty,
                OperateTime = t.OperateTime
            }).ToList()
        };
    }
}
