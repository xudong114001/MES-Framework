using MES.Application.Interfaces;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace MES.Infrastructure.Services;

public class BatchNumberService : IBatchNumberService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<BatchNumberService> _logger;

    public BatchNumberService(IConnectionMultiplexer redis, ILogger<BatchNumberService> logger)
    {
        _redis = redis;
        _logger = logger;
    }

    public async Task<string> GenerateBatchNoAsync(string prefix)
    {
        var now = DateTime.Now;
        var datePart = now.ToString("yyyyMMdd");
        var timePart = now.ToString("HHmmss");

        // 使用 Redis INCR 生成当日递增序号，保证唯一性
        var db = _redis.GetDatabase();
        var counterKey = $"batch:seq:{prefix}:{datePart}";
        var sequence = await db.StringIncrementAsync(counterKey);

        // 设置计数器 key 次日过期
        if (sequence == 1)
        {
            await db.KeyExpireAsync(counterKey, TimeSpan.FromDays(2));
        }

        // 格式：{prefix}{yyyyMMdd}{HHmmss}{3位序号}
        var batchNo = $"{prefix}{datePart}{timePart}{sequence:D3}";
        _logger.LogDebug("生成批次号：{BatchNo}", batchNo);
        return batchNo;
    }
}
