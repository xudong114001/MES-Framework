using MES.Application.Dtos;
using MES.Application.Interfaces;
using MES.Domain.Entities;
using MES.Domain.Repositories;
using MES.Domain.ValueObjects;

namespace MES.Application.Services;

public class BomService : IBomService
{
    private readonly IRepository<Bom> _repo;

    public BomService(IRepository<Bom> repo) => _repo = repo;

    private static BomDto MapToDto(Bom entity) => new()
    {
        Id = entity.Id,
        ProductId = entity.ProductId,
        MaterialId = entity.MaterialId,
        Quantity = entity.Quantity,
        ScrapRate = entity.ScrapRate,
        SeqNo = entity.SeqNo,
        ValidFrom = entity.ValidFrom,
        ValidTo = entity.ValidTo,
        Status = entity.Status,
        CreatedAt = entity.CreatedAt,
        CreatedBy = entity.CreatedBy,
        UpdatedAt = entity.UpdatedAt,
        UpdatedBy = entity.UpdatedBy
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

    public async Task<BomDto> CreateAsync(CreateBomRequest request)
    {
        var entity = new Bom
        {
            ProductId = request.ProductId,
            MaterialId = request.MaterialId,
            Quantity = new Quantity(request.Quantity),
            ScrapRate = request.ScrapRate,
            SeqNo = request.SeqNo,
            ValidFrom = request.ValidFrom,
            ValidTo = request.ValidTo,
            Status = request.Status
        };
        var created = await _repo.AddAsync(entity);
        return MapToDto(created);
    }

    public async Task UpdateAsync(long id, UpdateBomRequest request)
    {
        var existing = await _repo.GetByIdAsync(id);
        if (existing == null)
            throw new Domain.Exceptions.DomainException("BOM明细不存在");
        existing.ProductId = request.ProductId;
        existing.MaterialId = request.MaterialId;
        existing.Quantity = new Quantity(request.Quantity);
        existing.ScrapRate = request.ScrapRate;
        existing.SeqNo = request.SeqNo;
        existing.ValidFrom = request.ValidFrom;
        existing.ValidTo = request.ValidTo;
        existing.Status = request.Status;
        await _repo.UpdateAsync(existing);
    }

    public async Task DeleteAsync(long id)
    {
        var entity = await _repo.GetByIdAsync(id);
        if (entity == null)
            throw new Domain.Exceptions.DomainException("BOM明细不存在");
        entity.MarkAsDeleted();
        await _repo.UpdateAsync(entity);
    }
}
