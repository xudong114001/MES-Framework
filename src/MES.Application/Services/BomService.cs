using MES.Application.Dtos;
using MES.Application.Interfaces;
using MES.Domain.Entities;
using MES.Domain.Repositories;

namespace MES.Application.Services;

public class BomService : IBomService
{
    private readonly IRepository<Bom> _repo;

    public BomService(IRepository<Bom> repo) => _repo = repo;

    private static BomDto MapToDto(Bom entity) => new()
    {
        Id = entity.Id, ProductId = entity.ProductId, MaterialId = entity.MaterialId,
        Quantity = entity.Quantity, ScrapRate = entity.ScrapRate, SeqNo = entity.SeqNo,
        ValidFrom = entity.ValidFrom, ValidTo = entity.ValidTo, Status = entity.Status,
        CreatedAt = entity.CreatedAt, CreatedBy = entity.CreatedBy,
        UpdatedAt = entity.UpdatedAt, UpdatedBy = entity.UpdatedBy
    };

    public async Task<IEnumerable<BomDto>> GetAllAsync()
    {
        var list = await _repo.GetAllAsync();
        return list.Select(MapToDto);
    }

    public async Task<BomDto?> GetByIdAsync(long id)
    {
        var entity = await _repo.GetByIdAsync(id);
        return entity == null ? null : MapToDto(entity);
    }

    public async Task<IEnumerable<BomDto>> GetByProductIdAsync(long productId)
    {
        var list = await _repo.FindAsync(b => b.ProductId == productId);
        return list.Select(MapToDto);
    }

    public async Task<BomDto> CreateAsync(Bom entity)
    {
        var created = await _repo.AddAsync(entity);
        return MapToDto(created);
    }

    public async Task UpdateAsync(long id, Bom entity)
    {
        var existing = await _repo.GetByIdAsync(id);
        if (existing == null)
            throw new Domain.Exceptions.DomainException("BOM明细不存在");
        entity.Id = id;
        await _repo.UpdateAsync(entity);
    }

    public async Task DeleteAsync(long id)
    {
        var entity = await _repo.GetByIdAsync(id);
        if (entity == null)
            throw new Domain.Exceptions.DomainException("BOM明细不存在");
        await _repo.DeleteAsync(entity);
    }
}
