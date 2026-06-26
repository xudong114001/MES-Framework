using MES.Application.Dtos;

namespace MES.Application.Interfaces;

public interface IWorkshopService
{
    Task<IEnumerable<WorkshopDto>> GetAllAsync();
    Task<WorkshopDto?> GetByIdAsync(long id);
    Task<IEnumerable<WorkshopDto>> GetByFactoryIdAsync(long factoryId);
    Task<WorkshopDto> CreateAsync(CreateWorkshopRequest request);
    Task UpdateAsync(long id, UpdateWorkshopRequest request);
    Task DeleteAsync(long id);
}
