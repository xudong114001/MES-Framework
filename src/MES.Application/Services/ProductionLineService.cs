using MES.Application.Dtos;
using MES.Application.Interfaces;
using MES.Domain.Entities;
using MES.Domain.Repositories;

namespace MES.Application.Services;

public class ProductionLineService : IProductionLineService
{
    private readonly IRepository<ProductionLine> _repo;

    public ProductionLineService(IRepository<ProductionLine> repo) => _repo = repo;

    private static ProductionLineDto MapToDto(ProductionLine entity) => new()
    {
        Id = entity.Id,
        WorkshopId = entity.WorkshopId,
        Code = entity.Code,
        Name = entity.Name,
        LineType = entity.LineType,
        Status = entity.Status,
        CreatedAt = entity.CreatedAt,
        CreatedBy = entity.CreatedBy,
        UpdatedAt = entity.UpdatedAt,
        UpdatedBy = entity.UpdatedBy
    };

    public async Task<IEnumerable<ProductionLineDto>> GetAllAsync()
    {
        var list = await _repo.GetAllAsync();
        return list.Select(MapToDto);
    }

    public async Task<IEnumerable<ProductionLine>> GetAllLinesAsync()
    {
        return await _repo.FindAsync(l => l.Status);
    }

    public async Task<ProductionLineDto?> GetByIdAsync(long id)
    {
        var entity = await _repo.GetByIdAsync(id);
        return entity == null ? null : MapToDto(entity);
    }

    public async Task<IEnumerable<ProductionLineDto>> GetByWorkshopIdAsync(long workshopId)
    {
        var list = await _repo.FindAsync(pl => pl.WorkshopId == workshopId);
        return list.Select(MapToDto);
    }

    public async Task<ProductionLineDto> CreateAsync(CreateProductionLineRequest request)
    {
        var entity = new ProductionLine
        {
            WorkshopId = request.WorkshopId,
            Code = request.Code,
            Name = request.Name,
            LineType = request.LineType,
            Status = request.Status
        };
        var created = await _repo.AddAsync(entity);
        return MapToDto(created);
    }

    public async Task UpdateAsync(long id, UpdateProductionLineRequest request)
    {
        var existing = await _repo.GetByIdAsync(id);
        if (existing == null)
            throw new Domain.Exceptions.DomainException("产线不存在");
        existing.WorkshopId = request.WorkshopId;
        existing.Code = request.Code;
        existing.Name = request.Name;
        existing.LineType = request.LineType;
        existing.Status = request.Status;
        await _repo.UpdateAsync(existing);
    }

    public async Task DeleteAsync(long id)
    {
        var entity = await _repo.GetByIdAsync(id);
        if (entity == null)
            throw new Domain.Exceptions.DomainException("产线不存在");
        await _repo.DeleteAsync(entity);
    }
}
