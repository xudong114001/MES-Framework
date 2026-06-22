using MES.Domain.Interfaces;
using MES.Domain.Entities;
using MES.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace MES.Application.Services;

public class CachedRoutingService
{
    private readonly IRepository<Routing> _repo;
    private readonly ICacheService _cache;
    private static readonly TimeSpan CacheExpiry = TimeSpan.FromMinutes(5);

    public CachedRoutingService(IRepository<Routing> repo, ICacheService cache)
    {
        _repo = repo;
        _cache = cache;
    }

    public async Task<IEnumerable<Routing>> GetAllAsync()
    {
        var result = await _cache.GetOrSetAsync("routings:all", async () =>
        {
            var list = await _repo.Query().Include(r => r.Steps).ToListAsync();
            return list;
        }, CacheExpiry);
        return result ?? [];
    }

    public async Task<Routing?> GetByIdAsync(long id)
    {
        var cached = await _cache.GetAsync<Routing>($"routings:{id}");
        if (cached != null) return cached;

        var entity = await _repo.Query().Include(r => r.Steps).FirstOrDefaultAsync(r => r.Id == id);
        if (entity != null)
            await _cache.SetAsync($"routings:{id}", entity, CacheExpiry);
        return entity;
    }

    public async Task<IEnumerable<Routing>> GetByMaterialIdAsync(long materialId)
    {
        var result = await _cache.GetOrSetAsync($"routings:material:{materialId}", async () =>
        {
            var list = await _repo.Query()
                .Include(r => r.Steps)
                .Where(r => r.MaterialId == materialId)
                .ToListAsync();
            return list;
        }, CacheExpiry);
        return result ?? [];
    }

    public async Task<Routing> CreateAsync(Routing entity)
    {
        var created = await _repo.AddAsync(entity);
        await InvalidateCacheAsync(created.Id, created.MaterialId);
        return created;
    }

    public async Task UpdateAsync(Routing entity)
    {
        await _repo.UpdateAsync(entity);
        await InvalidateCacheAsync(entity.Id, entity.MaterialId);
    }

    public async Task DeleteAsync(Routing entity)
    {
        var materialId = entity.MaterialId;
        await _repo.DeleteAsync(entity);
        await InvalidateCacheAsync(entity.Id, materialId);
    }

    private async Task InvalidateCacheAsync(long id, long materialId)
    {
        await _cache.RemoveAsync($"routings:{id}");
        await _cache.RemoveAsync($"routings:material:{materialId}");
        await _cache.RemoveAsync("routings:all");
    }
}
