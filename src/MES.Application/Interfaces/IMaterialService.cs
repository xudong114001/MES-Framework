using MES.Application.Dtos;

namespace MES.Application.Interfaces;

public interface IMaterialService
{
    Task<IEnumerable<MaterialDto>> GetAllAsync();
    Task<MaterialDto?> GetByIdAsync(long id);
    Task<MaterialDto> CreateAsync(Domain.Entities.Material entity);
    Task UpdateAsync(long id, Domain.Entities.Material entity);
    Task DeleteAsync(long id);
}
