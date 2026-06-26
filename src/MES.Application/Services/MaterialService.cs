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
        Id = entity.Id,
        Code = entity.Code,
        Name = entity.Name,
        Spec = entity.Spec,
        Unit = entity.Unit,
        Category = entity.Category,
        BomLevel = entity.BomLevel,
        StockQty = entity.StockQty,
        Status = entity.Status,
        CreatedAt = entity.CreatedAt,
        CreatedBy = entity.CreatedBy,
        UpdatedAt = entity.UpdatedAt,
        UpdatedBy = entity.UpdatedBy
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

    public async Task<MaterialDto> CreateAsync(CreateMaterialRequest request)
    {
        var entity = new Material
        {
            Code = request.Code,
            Name = request.Name,
            Spec = request.Spec,
            Unit = request.Unit,
            Category = request.Category,
            BomLevel = request.BomLevel,
            StockQty = request.StockQty,
            Status = request.Status
        };
        var created = await _repo.AddAsync(entity);
        return MapToDto(created);
    }

    public async Task UpdateAsync(long id, UpdateMaterialRequest request)
    {
        var existing = await _repo.GetByIdAsync(id);
        if (existing == null)
            throw new Domain.Exceptions.DomainException("物料不存在");
        existing.Code = request.Code;
        existing.Name = request.Name;
        existing.Spec = request.Spec;
        existing.Unit = request.Unit;
        existing.Category = request.Category;
        existing.BomLevel = request.BomLevel;
        existing.StockQty = request.StockQty;
        existing.Status = request.Status;
        await _repo.UpdateAsync(existing);
    }

    public async Task DeleteAsync(long id)
    {
        var entity = await _repo.GetByIdAsync(id);
        if (entity == null)
            throw new Domain.Exceptions.DomainException("物料不存在");
        entity.MarkAsDeleted();
        await _repo.UpdateAsync(entity);
    }
}
