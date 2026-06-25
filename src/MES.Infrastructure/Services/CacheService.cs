using System.Text.Json;
using MES.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace MES.Infrastructure.Services;

public class CacheService : ICacheService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<CacheService> _logger;
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    public CacheService(IConnectionMultiplexer redis, ILogger<CacheService> logger)
    {
        _redis = redis;
        _logger = logger;
    }

    public async Task<T?> GetAsync<T>(string key) where T : class
    {
        var db = _redis.GetDatabase();
        var value = await db.StringGetAsync(key);
        if (value.IsNullOrEmpty) return null;
        return JsonSerializer.Deserialize<T>((string)value!, _jsonOptions);
    }

    public async Task<T?> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiry = null) where T : class
    {
        var cached = await GetAsync<T>(key);
        if (cached != null) return cached;

        var value = await factory();
        await SetAsync(key, value, expiry);
        return value;
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null) where T : class
    {
        var db = _redis.GetDatabase();
        var json = JsonSerializer.Serialize(value, _jsonOptions);
        await db.StringSetAsync(key, json, expiry ?? TimeSpan.FromMinutes(5));
    }

    public async Task RemoveAsync(string key)
    {
        var db = _redis.GetDatabase();
        await db.KeyDeleteAsync(key);
    }

    public async Task RemoveByPatternAsync(string pattern)
    {
        var server = _redis.GetServers().First();
        var keys = server.Keys(pattern: pattern);
        var db = _redis.GetDatabase();
        foreach (var key in keys)
        {
            await db.KeyDeleteAsync(key);
        }
    }

    public async Task<bool> SetIfNotExistsAsync(string key, TimeSpan expiry)
    {
        var db = _redis.GetDatabase();
        return await db.StringSetAsync(key, "1", expiry, When.NotExists);
    }

    public async Task<long> IncrementAsync(string key)
    {
        var db = _redis.GetDatabase();
        return await db.StringIncrementAsync(key);
    }

    public async Task SetExpiryAsync(string key, TimeSpan expiry)
    {
        var db = _redis.GetDatabase();
        await db.KeyExpireAsync(key, expiry);
    }
}
