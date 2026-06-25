using MES.Application.Dtos;
using MES.Application.Interfaces;
using MES.Domain.Entities;
using MES.Domain.Repositories;

namespace MES.Application.Services;

public class RoutingService : IRoutingService
{
    private readonly IRoutingRepository _repo;

    public RoutingService(IRoutingRepository repo) => _repo = repo;

    private static RoutingDto MapToDto(Routing entity) => new()
    {
        Id = entity.Id,
        MaterialId = entity.MaterialId,
        RoutingCode = entity.RoutingCode,
        RoutingName = entity.RoutingName,
        Version = entity.Version,
        Status = entity.Status,
        CreatedAt = entity.CreatedAt,
        CreatedBy = entity.CreatedBy,
        UpdatedAt = entity.UpdatedAt,
        UpdatedBy = entity.UpdatedBy
    };

    private static RoutingStepDto MapStepToDto(RoutingStep entity) => new()
    {
        Id = entity.Id,
        RoutingId = entity.RoutingId,
        StepNo = entity.StepNo,
        StepName = entity.StepName,
        WorkstationType = entity.WorkstationType,
        StandardTime = entity.StandardTime,
        IsQcPoint = entity.IsQcPoint,
        IsScrapPoint = entity.IsScrapPoint,
        CreatedAt = entity.CreatedAt,
        CreatedBy = entity.CreatedBy,
        UpdatedAt = entity.UpdatedAt,
        UpdatedBy = entity.UpdatedBy
    };

    private static RoutingDetailDto MapToDetailDto(Routing entity) => new()
    {
        Dto = MapToDto(entity),
        Steps = entity.Steps.Select(MapStepToDto)
    };

    public async Task<IEnumerable<RoutingDetailDto>> GetAllAsync()
    {
        var list = await _repo.GetAllWithStepsAsync();
        return list.Select(MapToDetailDto);
    }

    public async Task<RoutingDetailDto?> GetByIdAsync(long id)
    {
        var entity = await _repo.GetByIdWithStepsAsync(id);
        if (entity == null) return null;
        return MapToDetailDto(entity);
    }

    public async Task<IEnumerable<RoutingDetailDto>> GetByMaterialIdAsync(long materialId)
    {
        var list = await _repo.GetByMaterialIdWithStepsAsync(materialId);
        return list.Select(MapToDetailDto);
    }

    public async Task<RoutingDto> CreateAsync(CreateRoutingRequest request)
    {
        var entity = Routing.Create(
            request.MaterialId,
            request.RoutingCode,
            request.RoutingName,
            request.Version,
            request.Status);
        var created = await _repo.AddAsync(entity);
        return MapToDto(created);
    }

    public async Task UpdateAsync(long id, UpdateRoutingRequest request)
    {
        var existing = await _repo.GetByIdAsync(id);
        if (existing == null)
            throw new Domain.Exceptions.DomainException("工艺路线不存在");
        existing.UpdateInfo(
            request.MaterialId,
            request.RoutingCode,
            request.RoutingName,
            request.Version,
            request.Status);
        await _repo.UpdateAsync(existing);
    }

    public async Task DeleteAsync(long id)
    {
        var entity = await _repo.GetByIdAsync(id);
        if (entity == null)
            throw new Domain.Exceptions.DomainException("工艺路线不存在");
        await _repo.DeleteAsync(entity);
    }
}
