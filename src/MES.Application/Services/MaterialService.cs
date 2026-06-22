using MES.Application.Dtos;
using MES.Application.Interfaces;
using MES.Domain.Entities;
using MES.Domain.Repositories;

namespace MES.Application.Services;

public class MaterialService : IMaterialService
{
    private readonly IRepository<Material> _repo;

    public MaterialService(IRepository<Material> repo) => _repo = repo;

    private static MaterialDto MapToDto(Material entity) => new()
    {
        Id = entity.Id, Code = entity.Code, Name = entity.Name,
        Spec = entity.Spec, Unit = entity.Unit, Category = entity.Category,
        BomLevel = entity.BomLevel, StockQty = entity.StockQty, Status = entity.Status,
        CreatedAt = entity.CreatedAt, CreatedBy = entity.CreatedBy,
        UpdatedAt = entity.UpdatedAt, UpdatedBy = entity.UpdatedBy
    };

    public async Task<IEnumerable<MaterialDto>> GetAllAsync()
    {
        var list = await _repo.GetAllAsync();
        return list.Select(MapToDto);
    }

    public async Task<MaterialDto?> GetByIdAsync(long id)
    {
        var entity = await _repo.GetByIdAsync(id);
        return entity == null ? null : MapToDto(entity);
    }

    public async Task<MaterialDto> CreateAsync(Material entity)
    {
        var created = await _repo.AddAsync(entity);
        return MapToDto(created);
    }

    public async Task UpdateAsync(long id, Material entity)
    {
        var existing = await _repo.GetByIdAsync(id);
        if (existing == null)
            throw new Domain.Exceptions.DomainException("物料不存在");

        entity.Id = id;
        entity.CreatedAt = existing.CreatedAt;
        entity.CreatedBy = existing.CreatedBy;
        entity.UpdatedAt = DateTime.UtcNow;
        await _repo.UpdateAsync(entity);
    }

    public async Task DeleteAsync(long id)
    {
        var entity = await _repo.GetByIdAsync(id);
        if (entity == null)
            throw new Domain.Exceptions.DomainException("物料不存在");
        await _repo.DeleteAsync(entity);
    }
}
