using MES.Application.Dtos;

namespace MES.Application.Interfaces;

public interface ITraceService
{
    /// <summary>按批次查询</summary>
    Task<TraceResultDto> TraceByBatchAsync(string batchNo);

    /// <summary>按序列号查询</summary>
    Task<TraceResultDto> TraceBySerialAsync(string serialNo);

    /// <summary>正向追溯</summary>
    Task<TraceResultDto> TraceForwardAsync(long materialId, string batchNo);

    /// <summary>反向追溯</summary>
    Task<TraceResultDto> TraceBackwardAsync(string serialNo);
}
