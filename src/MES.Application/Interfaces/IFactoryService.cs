using MES.Application.Dtos;

namespace MES.Application.Interfaces;

public interface IFactoryService
{
    Task<IEnumerable<FactoryDto>> GetAllAsync();
    Task<FactoryDto?> GetByIdAsync(long id);
    Task<FactoryDto> CreateAsync(Domain.Entities.Factory entity);
    Task UpdateAsync(long id, Domain.Entities.Factory entity);
    Task DeleteAsync(long id);
}
