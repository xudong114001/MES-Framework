using MES.Application.Dtos;
using MES.Application.Interfaces;
using MES.Domain.Entities;
using MES.Domain.Repositories;

namespace MES.Application.Services;

public class FactoryService : IFactoryService
{
    private readonly IRepository<Factory> _repo;

    public FactoryService(IRepository<Factory> repo) => _repo = repo;

    private static FactoryDto MapToDto(Factory entity) => new()
    {
        Id = entity.Id, Code = entity.Code, Name = entity.Name,
        Address = entity.Address, Status = entity.Status,
        CreatedAt = entity.CreatedAt, CreatedBy = entity.CreatedBy,
        UpdatedAt = entity.UpdatedAt, UpdatedBy = entity.UpdatedBy
    };

    public async Task<IEnumerable<FactoryDto>> GetAllAsync()
    {
        var list = await _repo.GetAllAsync();
        return list.Select(MapToDto);
    }

    public async Task<FactoryDto?> GetByIdAsync(long id)
    {
        var entity = await _repo.GetByIdAsync(id);
        return entity == null ? null : MapToDto(entity);
    }

    public async Task<FactoryDto> CreateAsync(Factory entity)
    {
        var created = await _repo.AddAsync(entity);
        return MapToDto(created);
    }

    public async Task UpdateAsync(long id, Factory entity)
    {
        var existing = await _repo.GetByIdAsync(id);
        if (existing == null)
            throw new Domain.Exceptions.DomainException("工厂不存在");
        entity.Id = id;
        await _repo.UpdateAsync(entity);
    }

    public async Task DeleteAsync(long id)
    {
        var entity = await _repo.GetByIdAsync(id);
        if (entity == null)
            throw new Domain.Exceptions.DomainException("工厂不存在");
        await _repo.DeleteAsync(entity);
    }
}
