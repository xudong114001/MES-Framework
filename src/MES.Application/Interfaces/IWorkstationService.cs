using MES.Application.Dtos;

namespace MES.Application.Interfaces;

public interface IWorkstationService
{
    Task<IEnumerable<WorkstationDto>> GetAllAsync();
    Task<WorkstationDto?> GetByIdAsync(long id);
    Task<IEnumerable<WorkstationDto>> GetByLineIdAsync(long lineId);
    Task<WorkstationDto> CreateAsync(Domain.Entities.Workstation entity);
    Task UpdateAsync(long id, Domain.Entities.Workstation entity);
    Task DeleteAsync(long id);
}
