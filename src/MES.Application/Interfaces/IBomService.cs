using MES.Application.Dtos;

namespace MES.Application.Interfaces;

public interface IBomService
{
    Task<IEnumerable<BomDto>> GetAllAsync();
    Task<BomDto?> GetByIdAsync(long id);
    Task<IEnumerable<BomDto>> GetByProductIdAsync(long productId);
    Task<BomDto> CreateAsync(Domain.Entities.Bom entity);
    Task UpdateAsync(long id, Domain.Entities.Bom entity);
    Task DeleteAsync(long id);
}
