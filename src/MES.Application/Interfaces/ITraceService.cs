namespace MES.Application.Interfaces;

public interface ITraceService
{
    /// <summary>按批次查询</summary>
    Task<object> TraceByBatchAsync(string batchNo);
    
    /// <summary>按序列号查询</summary>
    Task<object> TraceBySerialAsync(string serialNo);
    
    /// <summary>正向追溯</summary>
    Task<object> TraceForwardAsync(long materialId, string batchNo);
    
    /// <summary>反向追溯</summary>
    Task<object> TraceBackwardAsync(string serialNo);
}
