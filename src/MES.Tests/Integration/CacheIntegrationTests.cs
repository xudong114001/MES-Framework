using System;
using System.Threading.Tasks;
using StackExchange.Redis;
using Xunit;
using Xunit.Abstractions;
using Assert = Xunit.Assert;

namespace MES.Tests.Integration;

public class CacheIntegrationTests : IntegrationTestBase
{
    private readonly ITestOutputHelper _output;

    public CacheIntegrationTests(ITestOutputHelper output)
    {
        _output = output;
    }

    /// <summary>
    /// 测试：缓存击穿防护
    /// 当缓存失效时，只允许一个请求去加载数据，其他请求等待或获取旧值
    /// 使用 Redis 分布式锁实现
    /// </summary>
    [Fact]
    public async Task CacheBreakdown_WithDistributedLock_ShouldPreventStampede()
    {
        const string cacheKey = "mes:material:stock:MAT-001";
        const string lockKey = "mes:lock:material:stock:MAT-001";
        var lockValue = Guid.NewGuid().ToString();

        // 预设缓存值（模拟已缓存的数据）
        await RedisDb.StringSetAsync(cacheKey, "1000", TimeSpan.FromSeconds(2));

        // 等待缓存过期
        await Task.Delay(2500);

        // 模拟 50 个并发请求同时发现缓存过期
        const int concurrentRequests = 50;
        var tasks = new Task<bool>[concurrentRequests];
        var dbLoadCount = 0; // 只应有 1 个请求真正去加载数据

        for (int i = 0; i < concurrentRequests; i++)
        {
            tasks[i] = Task.Run(async () =>
            {
                // 先查缓存
                var cachedValue = await RedisDb.StringGetAsync(cacheKey);
                if (cachedValue.HasValue)
                    return true;

                // 缓存未命中，尝试获取分布式锁
                var acquired = await RedisDb.StringSetAsync(
                    lockKey,
                    lockValue,
                    TimeSpan.FromSeconds(5),
                    When.NotExists);

                if (acquired)
                {
                    try
                    {
                        // 模拟数据库加载
                        Interlocked.Increment(ref dbLoadCount);
                        await Task.Delay(100); // 模拟 IO 延迟

                        // 写入缓存
                        await RedisDb.StringSetAsync(cacheKey, "999", TimeSpan.FromSeconds(60));
                        return true;
                    }
                    finally
                    {
                        // 释放锁（使用 Lua 脚本确保只删自己的锁）
                        var luaScript = @"
                            if redis.call('get', KEYS[1]) == ARGV[1] then
                                return redis.call('del', KEYS[1])
                            else
                                return 0
                            end";
                        await RedisDb.ScriptEvaluateAsync(luaScript,
                            new RedisKey[] { lockKey },
                            new RedisValue[] { lockValue });
                    }
                }
                else
                {
                    // 等待锁释放后重试获取缓存
                    for (int retry = 0; retry < 10; retry++)
                    {
                        await Task.Delay(50);
                        cachedValue = await RedisDb.StringGetAsync(cacheKey);
                        if (cachedValue.HasValue)
                            return true;
                    }
                    return false;
                }
            });
        }

        var results = await Task.WhenAll(tasks);

        // Assert: 只有 1 个请求真正访问数据库（缓存击穿防护生效）
        Assert.Equal(1, dbLoadCount);

        // 所有请求最终都获取到了缓存值
        Assert.All(results, result => Assert.True(result));

        // 验证缓存值正确
        var finalValue = await RedisDb.StringGetAsync(cacheKey);
        Assert.Equal("999", finalValue.ToString());

        _output.WriteLine($"缓存击穿防护测试通过：dbLoadCount={dbLoadCount}, successCount={results.Count(r => r)}");
    }

    /// <summary>
    /// 测试：缓存过期时间 —— 设置 TTL 后，过期应自动删除
    /// </summary>
    [Fact]
    public async Task CacheExpiration_ShouldAutoDeleteAfterTTL()
    {
        const string cacheKey = "mes:config:batch_prefix";

        // 设置 2 秒过期
        await RedisDb.StringSetAsync(cacheKey, "BATCH-2026", TimeSpan.FromSeconds(2));

        // 立即读取应存在
        var value1 = await RedisDb.StringGetAsync(cacheKey);
        Assert.Equal("BATCH-2026", value1.ToString());

        // 检查 TTL > 0
        var ttl1 = await RedisDb.KeyTimeToLiveAsync(cacheKey);
        Assert.NotNull(ttl1);
        Assert.True(ttl1.Value.TotalSeconds > 0 && ttl1.Value.TotalSeconds <= 2);

        // 等待过期
        await Task.Delay(2500);

        // 过期后应不存在
        var value2 = await RedisDb.StringGetAsync(cacheKey);
        Assert.False(value2.HasValue, "缓存过期后应自动删除");

        _output.WriteLine("缓存过期时间测试通过");
    }

    /// <summary>
    /// 测试：分布式锁 —— 互斥性、可重入、超时释放
    /// </summary>
    [Fact]
    public async Task DistributedLock_ShouldBeMutuallyExclusive()
    {
        const string lockKey = "mes:lock:work_order:generate_batch";
        var lockHolder1 = Guid.NewGuid().ToString();
        var lockHolder2 = Guid.NewGuid().ToString();

        // 1. 第一个客户端获取锁
        var acquired1 = await RedisDb.StringSetAsync(
            lockKey, lockHolder1, TimeSpan.FromSeconds(10), When.NotExists);
        Assert.True(acquired1, "第一个客户端应成功获取锁");

        // 2. 第二个客户端尝试获取同一把锁应失败
        var acquired2 = await RedisDb.StringSetAsync(
            lockKey, lockHolder2, TimeSpan.FromSeconds(10), When.NotExists);
        Assert.False(acquired2, "第二个客户端不应获取已被持有的锁");

        // 3. 验证锁的持有者是 client1
        var lockValue = await RedisDb.StringGetAsync(lockKey);
        Assert.Equal(lockHolder1, lockValue.ToString());

        // 4. 释放锁（使用 Lua 脚本确保原子性，只有持有者才能释放）
        var releaseScript = @"
            if redis.call('get', KEYS[1]) == ARGV[1] then
                return redis.call('del', KEYS[1])
            else
                return 0
            end";

        // 非持有者尝试释放应失败
        var releaseResult1 = await RedisDb.ScriptEvaluateAsync(releaseScript,
            new RedisKey[] { lockKey },
            new RedisValue[] { lockHolder2 });
        Assert.Equal(0, (long)releaseResult1);

        // 锁仍然存在
        Assert.True(await RedisDb.KeyExistsAsync(lockKey));

        // 持有者释放应成功
        var releaseResult2 = await RedisDb.ScriptEvaluateAsync(releaseScript,
            new RedisKey[] { lockKey },
            new RedisValue[] { lockHolder1 });
        Assert.Equal(1, (long)releaseResult2);

        // 锁已被释放
        Assert.False(await RedisDb.KeyExistsAsync(lockKey));

        // 5. 锁释放后，其他客户端可以获取
        var acquired3 = await RedisDb.StringSetAsync(
            lockKey, lockHolder2, TimeSpan.FromSeconds(10), When.NotExists);
        Assert.True(acquired3, "锁释放后其他客户端应能获取");

        // 6. 测试锁超时自动释放
        var timeoutLockKey = "mes:lock:timeout_test";
        var timeoutHolder = Guid.NewGuid().ToString();
        await RedisDb.StringSetAsync(timeoutLockKey, timeoutHolder, TimeSpan.FromSeconds(1));

        Assert.True(await RedisDb.KeyExistsAsync(timeoutLockKey));

        await Task.Delay(1500); // 等待超时

        Assert.False(await RedisDb.KeyExistsAsync(timeoutLockKey), "锁超时后应自动释放");

        _output.WriteLine("分布式锁测试通过");
    }

    /// <summary>
    /// 测试：分布式锁并发场景 —— 多个客户端竞争同一把锁
    /// </summary>
    [Fact]
    public async Task DistributedLock_ConcurrentAcquire_OnlyOneShouldSucceed()
    {
        const string lockKey = "mes:lock:concurrent_test";
        const int competitorCount = 20;
        var results = new System.Collections.Concurrent.ConcurrentBag<(int clientId, bool acquired)>();

        var tasks = Enumerable.Range(0, competitorCount).Select(async clientId =>
        {
            var lockValue = Guid.NewGuid().ToString();
            var acquired = await RedisDb.StringSetAsync(
                lockKey, lockValue, TimeSpan.FromSeconds(30), When.NotExists);
            results.Add((clientId, acquired));

            if (acquired)
            {
                // 持有锁一小段时间
                await Task.Delay(100);
                // 释放锁（简单方式：直接删除，忽略值是否匹配）
                await RedisDb.KeyDeleteAsync(lockKey);
            }
        });

        await Task.WhenAll(tasks);

        // Assert: 只有一个客户端成功获取锁
        var acquiredCount = results.Count(r => r.acquired);
        Assert.Equal(1, acquiredCount);

        _output.WriteLine($"并发锁竞争测试通过：{competitorCount} 个竞争者中 {acquiredCount} 个获取成功");
    }

    /// <summary>
    /// 测试：缓存 Hash 结构 —— 适用于 MES 物料库存缓存
    /// </summary>
    [Fact]
    public async Task CacheHash_ShouldSupportMaterialStockCaching()
    {
        const string hashKey = "mes:stock:material";

        // 设置多个物料的库存
        await RedisDb.HashSetAsync(hashKey, [
            new HashEntry("MAT-001", 1000),
            new HashEntry("MAT-002", 500),
            new HashEntry("MAT-003", 200)
        ]);

        // 读取单个字段
        var stock1 = await RedisDb.HashGetAsync(hashKey, "MAT-001");
        Assert.Equal(1000, (long)stock1);

        // 读取所有字段
        var allStock = await RedisDb.HashGetAllAsync(hashKey);
        Assert.Equal(3, allStock.Length);

        // 原子递减（模拟出库扣减）
        var newStock = await RedisDb.HashDecrementAsync(hashKey, "MAT-001", 50);
        Assert.Equal(950, newStock);

        // 验证递减后的值
        var afterDecrement = await RedisDb.HashGetAsync(hashKey, "MAT-001");
        Assert.Equal(950, (long)afterDecrement);

        _output.WriteLine("Hash 缓存结构测试通过");
    }
}
