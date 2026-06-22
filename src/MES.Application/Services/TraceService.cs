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
    public async Task<object> TraceByBatchAsync(string batchNo)
    {
        var traces = await _traceRepo.FindAsync(t => t.BatchNo == batchNo);
        var list = traces.ToList();
        return new
        {
            batchNo,
            records = list.Select(t => new
            {
                t.Id,
                t.MaterialId,
                t.Direction,
                t.Qty,
                t.Operator,
                t.OperateTime,
                t.WorkOrderId,
                t.Remark
            })
        };
    }

    // 按序列号查询
    public async Task<object> TraceBySerialAsync(string serialNo)
    {
        var traces = await _traceRepo.FindAsync(t => t.SerialNo == serialNo);
        return new { serialNo, records = traces.ToList() };
    }

    // 正向追溯：原料批次 → 消耗到哪些工单 → 产出哪些成品
    public async Task<object> TraceForwardAsync(long materialId, string batchNo)
    {
        var traces = await _traceRepo.FindAsync(t => t.MaterialId == materialId && t.BatchNo == batchNo && t.Direction == "CONSUME");
        return new { materialId, batchNo, consumeRecords = traces.ToList() };
    }

    // 反向追溯：成品序列号 → 用了哪些原料批次
    public async Task<object> TraceBackwardAsync(string serialNo)
    {
        var traces = await _traceRepo.FindAsync(t => t.SerialNo == serialNo);
        return new { serialNo, records = traces.ToList() };
    }
}
