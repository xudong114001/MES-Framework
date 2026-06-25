using MES.Domain.Interfaces;
using MES.Domain.Entities;
using MES.Domain.Repositories;

namespace MES.Application.Services;

public class CachedMaterialService
{
    private readonly IRepository<Material> _repo;
    private readonly ICacheService _cache;
    private static readonly TimeSpan CacheExpiry = TimeSpan.FromMinutes(5);

    public CachedMaterialService(IRepository<Material> repo, ICacheService cache)
    {
        _repo = repo;
        _cache = cache;
    }

    public async Task<IEnumerable<Material>> GetAllAsync()
    {
        var result = await _cache.GetOrSetAsync("materials:all", async () =>
        {
            var list = await _repo.GetAllAsync();
            return list.ToList();
        }, CacheExpiry);
        return result ?? [];
    }

    public async Task<Material?> GetByIdAsync(long id)
    {
        var cached = await _cache.GetAsync<Material>($"materials:{id}");
        if (cached != null) return cached;

        var entity = await _repo.GetByIdAsync(id);
        if (entity != null)
            await _cache.SetAsync($"materials:{id}", entity, CacheExpiry);
        return entity;
    }

    public async Task<Material> CreateAsync(Material entity)
    {
        var created = await _repo.AddAsync(entity);
        await InvalidateCacheAsync(created.Id);
        return created;
    }

    public async Task UpdateAsync(Material entity)
    {
        await _repo.UpdateAsync(entity);
        await InvalidateCacheAsync(entity.Id);
    }

    public async Task DeleteAsync(Material entity)
    {
        await _repo.DeleteAsync(entity);
        await InvalidateCacheAsync(entity.Id);
    }

    private async Task InvalidateCacheAsync(long id)
    {
        await _cache.RemoveAsync($"materials:{id}");
        await _cache.RemoveAsync("materials:all");
    }
}
