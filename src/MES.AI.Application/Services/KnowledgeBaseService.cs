using MES.AI.Application.Dtos;
using MES.AI.Application.Interfaces;
using MES.AI.Domain.Entities;
using MES.AI.Domain.Enums;
using MES.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MES.AI.Application.Services;

public class KnowledgeBaseService : IKnowledgeBaseService
{
    private readonly IRepository<KnowledgeEntry> _repo;
    private readonly ILogger<KnowledgeBaseService> _logger;

    public KnowledgeBaseService(
        IRepository<KnowledgeEntry> repo,
        ILogger<KnowledgeBaseService> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public async Task<KnowledgeQueryResult> SearchAsync(string? query, int? category, int page = 1, int pageSize = 20)
    {
        var queryable = _repo.Query().Where(e => !e.IsDeleted);

        if (!string.IsNullOrWhiteSpace(query))
        {
            var q = query.Trim();
            queryable = queryable.Where(e =>
                e.Title.Contains(q) ||
                e.Content.Contains(q) ||
                (e.Keywords != null && e.Keywords.Contains(q)));
        }

        if (category.HasValue)
        {
            queryable = queryable.Where(e => e.Category == category.Value);
        }

        var totalCount = await queryable.CountAsync();

        var items = await queryable
            .OrderByDescending(e => e.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new KnowledgeQueryResult
        {
            Items = items.Select(MapToDto).ToList(),
            TotalCount = totalCount,
            Query = query,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<List<KnowledgeEntryDto>> GetAllAsync(int? category, int page = 1, int pageSize = 20)
    {
        var queryable = _repo.Query().Where(e => !e.IsDeleted);

        if (category.HasValue)
        {
            queryable = queryable.Where(e => e.Category == category.Value);
        }

        var items = await queryable
            .OrderByDescending(e => e.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return items.Select(MapToDto).ToList();
    }

    public async Task<KnowledgeEntryDto?> GetByIdAsync(long id)
    {
        var entity = await _repo.GetByIdAsync(id);
        return entity == null || entity.IsDeleted ? null : MapToDto(entity);
    }

    public async Task<KnowledgeEntryDto> AddAsync(KnowledgeEntryDto dto)
    {
        var entity = new KnowledgeEntry
        {
            Category = dto.Category,
            Title = dto.Title,
            Content = dto.Content,
            Keywords = dto.Keywords,
            MaterialId = dto.MaterialId,
            EquipmentId = dto.EquipmentId
        };

        var created = await _repo.AddAsync(entity);
        await _repo.SaveChangesAsync();

        _logger.LogInformation("Knowledge entry added: {Id} - {Title}", created.Id, created.Title);

        return MapToDto(created);
    }

    public async Task<KnowledgeEntryDto?> UpdateAsync(long id, KnowledgeEntryDto dto)
    {
        var entity = await _repo.GetByIdAsync(id);
        if (entity == null || entity.IsDeleted) return null;

        entity.Category = dto.Category;
        entity.Title = dto.Title;
        entity.Content = dto.Content;
        entity.Keywords = dto.Keywords;
        entity.MaterialId = dto.MaterialId;
        entity.EquipmentId = dto.EquipmentId;
        entity.UpdatedAt = DateTime.UtcNow;

        await _repo.UpdateAsync(entity);
        await _repo.SaveChangesAsync();

        _logger.LogInformation("Knowledge entry updated: {Id} - {Title}", id, dto.Title);

        return MapToDto(entity);
    }

    public async Task<bool> DeleteAsync(long id)
    {
        var entity = await _repo.GetByIdAsync(id);
        if (entity == null || entity.IsDeleted) return false;

        entity.IsDeleted = true;
        entity.UpdatedAt = DateTime.UtcNow;

        await _repo.UpdateAsync(entity);
        await _repo.SaveChangesAsync();

        _logger.LogInformation("Knowledge entry deleted: {Id}", id);

        return true;
    }

    private static KnowledgeEntryDto MapToDto(KnowledgeEntry entity)
    {
        return new KnowledgeEntryDto
        {
            Id = entity.Id,
            Category = entity.Category,
            Title = entity.Title,
            Content = entity.Content,
            Keywords = entity.Keywords,
            MaterialId = entity.MaterialId,
            EquipmentId = entity.EquipmentId
        };
    }
}
