using MES.Application.Dtos;

namespace MES.Application.Interfaces;

public interface IWorkshopService
{
    Task<IEnumerable<WorkshopDto>> GetAllAsync();
    Task<WorkshopDto?> GetByIdAsync(long id);
    Task<IEnumerable<WorkshopDto>> GetByFactoryIdAsync(long factoryId);
    Task<WorkshopDto> CreateAsync(Domain.Entities.Workshop entity);
    Task UpdateAsync(long id, Domain.Entities.Workshop entity);
    Task DeleteAsync(long id);
}
