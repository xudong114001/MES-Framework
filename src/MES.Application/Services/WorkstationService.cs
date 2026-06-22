using MES.Application.Dtos;
using MES.Application.Interfaces;
using MES.Domain.Entities;
using MES.Domain.Repositories;

namespace MES.Application.Services;

public class WorkstationService : IWorkstationService
{
    private readonly IRepository<Workstation> _repo;

    public WorkstationService(IRepository<Workstation> repo) => _repo = repo;

    private static WorkstationDto MapToDto(Workstation entity) => new()
    {
        Id = entity.Id, LineId = entity.LineId, Code = entity.Code, Name = entity.Name,
        SeqNo = entity.SeqNo, Status = entity.Status,
        CreatedAt = entity.CreatedAt, CreatedBy = entity.CreatedBy,
        UpdatedAt = entity.UpdatedAt, UpdatedBy = entity.UpdatedBy
    };

    public async Task<IEnumerable<WorkstationDto>> GetAllAsync()
    {
        var list = await _repo.GetAllAsync();
        return list.Select(MapToDto);
    }

    public async Task<WorkstationDto?> GetByIdAsync(long id)
    {
        var entity = await _repo.GetByIdAsync(id);
        return entity == null ? null : MapToDto(entity);
    }

    public async Task<IEnumerable<WorkstationDto>> GetByLineIdAsync(long lineId)
    {
        var list = await _repo.FindAsync(ws => ws.LineId == lineId);
        return list.Select(MapToDto);
    }

    public async Task<WorkstationDto> CreateAsync(Workstation entity)
    {
        var created = await _repo.AddAsync(entity);
        return MapToDto(created);
    }

    public async Task UpdateAsync(long id, Workstation entity)
    {
        var existing = await _repo.GetByIdAsync(id);
        if (existing == null)
            throw new Domain.Exceptions.DomainException("工位不存在");
        entity.Id = id;
        await _repo.UpdateAsync(entity);
    }

    public async Task DeleteAsync(long id)
    {
        var entity = await _repo.GetByIdAsync(id);
        if (entity == null)
            throw new Domain.Exceptions.DomainException("工位不存在");
        await _repo.DeleteAsync(entity);
    }
}
