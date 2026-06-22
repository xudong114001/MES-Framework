using MES.Application.Dtos;

namespace MES.Application.Interfaces;

public interface IRoutingService
{
    Task<IEnumerable<object>> GetAllAsync();
    Task<object?> GetByIdAsync(long id);
    Task<IEnumerable<object>> GetByMaterialIdAsync(long materialId);
    Task<RoutingDto> CreateAsync(Domain.Entities.Routing entity);
    Task UpdateAsync(long id, Domain.Entities.Routing entity);
    Task DeleteAsync(long id);
}
