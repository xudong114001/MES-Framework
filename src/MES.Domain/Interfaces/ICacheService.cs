namespace MES.Domain.Interfaces;

public interface ICacheService
{
    Task<T?> GetAsync<T>(string key) where T : class;
    Task<T?> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiry = null) where T : class;
    Task SetAsync<T>(string key, T value, TimeSpan? expiry = null) where T : class;
    Task RemoveAsync(string key);
    Task RemoveByPatternAsync(string pattern);

    /// <summary>
    /// 防重复提交：仅当 key 不存在时设置值并返回 true，否则返回 false
    /// </summary>
    Task<bool> SetIfNotExistsAsync(string key, TimeSpan expiry);
}