using MES.Application.Dtos;
using MES.Application.Interfaces;
using MES.Domain.Entities;
using MES.Domain.Repositories;

namespace MES.Application.Services;

public class WorkshopService : IWorkshopService
{
    private readonly IRepository<Workshop> _repo;

    public WorkshopService(IRepository<Workshop> repo) => _repo = repo;

    private static WorkshopDto MapToDto(Workshop entity) => new()
    {
        Id = entity.Id,
        FactoryId = entity.FactoryId,
        Code = entity.Code,
        Name = entity.Name,
        Status = entity.Status,
        CreatedAt = entity.CreatedAt,
        CreatedBy = entity.CreatedBy,
        UpdatedAt = entity.UpdatedAt,
        UpdatedBy = entity.UpdatedBy
    };

    public async Task<IEnumerable<WorkshopDto>> GetAllAsync()
    {
        var list = await _repo.GetAllAsync();
        return list.Select(MapToDto);
    }

    public async Task<WorkshopDto?> GetByIdAsync(long id)
    {
        var entity = await _repo.GetByIdAsync(id);
        return entity == null ? null : MapToDto(entity);
    }

    public async Task<IEnumerable<WorkshopDto>> GetByFactoryIdAsync(long factoryId)
    {
        var list = await _repo.FindAsync(w => w.FactoryId == factoryId);
        return list.Select(MapToDto);
    }

    public async Task<WorkshopDto> CreateAsync(Workshop entity)
    {
        var created = await _repo.AddAsync(entity);
        return MapToDto(created);
    }

    public async Task UpdateAsync(long id, Workshop entity)
    {
        var existing = await _repo.GetByIdAsync(id);
        if (existing == null)
            throw new Domain.Exceptions.DomainException("车间不存在");
        existing.FactoryId = entity.FactoryId;
        existing.Code = entity.Code;
        existing.Name = entity.Name;
        existing.Status = entity.Status;
        await _repo.UpdateAsync(existing);
    }

    public async Task DeleteAsync(long id)
    {
        var entity = await _repo.GetByIdAsync(id);
        if (entity == null)
            throw new Domain.Exceptions.DomainException("车间不存在");
        await _repo.DeleteAsync(entity);
    }
}
