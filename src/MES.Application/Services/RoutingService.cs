using Microsoft.EntityFrameworkCore;
using MES.Application.Dtos;
using MES.Application.Interfaces;
using MES.Domain.Entities;
using MES.Domain.Repositories;

namespace MES.Application.Services;

public class RoutingService : IRoutingService
{
    private readonly IRepository<Routing> _repo;

    public RoutingService(IRepository<Routing> repo) => _repo = repo;

    private static RoutingDto MapToDto(Routing entity) => new()
    {
        Id = entity.Id, MaterialId = entity.MaterialId,
        RoutingCode = entity.RoutingCode, RoutingName = entity.RoutingName,
        Version = entity.Version, Status = entity.Status,
        CreatedAt = entity.CreatedAt, CreatedBy = entity.CreatedBy,
        UpdatedAt = entity.UpdatedAt, UpdatedBy = entity.UpdatedBy
    };

    private static RoutingStepDto MapStepToDto(RoutingStep entity) => new()
    {
        Id = entity.Id, RoutingId = entity.RoutingId,
        StepNo = entity.StepNo, StepName = entity.StepName,
        WorkstationType = entity.WorkstationType, StandardTime = entity.StandardTime,
        IsQcPoint = entity.IsQcPoint, IsScrapPoint = entity.IsScrapPoint,
        CreatedAt = entity.CreatedAt, CreatedBy = entity.CreatedBy,
        UpdatedAt = entity.UpdatedAt, UpdatedBy = entity.UpdatedBy
    };

    public async Task<IEnumerable<object>> GetAllAsync()
    {
        var list = await _repo.Query().Include(r => r.Steps).ToListAsync();
        return list.Select(r => new
        {
            Dto = MapToDto(r),
            Steps = r.Steps.Select(MapStepToDto)
        });
    }

    public async Task<object?> GetByIdAsync(long id)
    {
        var entity = await _repo.Query().Include(r => r.Steps).FirstOrDefaultAsync(r => r.Id == id);
        if (entity == null) return null;
        return new
        {
            Dto = MapToDto(entity),
            Steps = entity.Steps.Select(MapStepToDto)
        };
    }

    public async Task<IEnumerable<object>> GetByMaterialIdAsync(long materialId)
    {
        var list = await _repo.Query()
            .Include(r => r.Steps)
            .Where(r => r.MaterialId == materialId)
            .ToListAsync();
        return list.Select(r => new
        {
            Dto = MapToDto(r),
            Steps = r.Steps.Select(MapStepToDto)
        });
    }

    public async Task<RoutingDto> CreateAsync(Routing entity)
    {
        var created = await _repo.AddAsync(entity);
        return MapToDto(created);
    }

    public async Task UpdateAsync(long id, Routing entity)
    {
        var existing = await _repo.GetByIdAsync(id);
        if (existing == null)
            throw new Domain.Exceptions.DomainException("工艺路线不存在");
        entity.Id = id;
        await _repo.UpdateAsync(entity);
    }

    public async Task DeleteAsync(long id)
    {
        var entity = await _repo.GetByIdAsync(id);
        if (entity == null)
            throw new Domain.Exceptions.DomainException("工艺路线不存在");
        await _repo.DeleteAsync(entity);
    }
}
