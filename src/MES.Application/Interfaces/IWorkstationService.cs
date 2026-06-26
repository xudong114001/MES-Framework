using MES.Application.Dtos;

namespace MES.Application.Interfaces;

public interface IWorkstationService
{
    Task<IEnumerable<WorkstationDto>> GetAllAsync();
    Task<WorkstationDto?> GetByIdAsync(long id);
    Task<IEnumerable<WorkstationDto>> GetByLineIdAsync(long lineId);
    Task<WorkstationDto> CreateAsync(CreateWorkstationRequest request);
    Task UpdateAsync(long id, UpdateWorkstationRequest request);
    Task DeleteAsync(long id);
}
