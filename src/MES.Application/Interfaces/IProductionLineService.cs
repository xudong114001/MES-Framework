using MES.Application.Dtos;

namespace MES.Application.Interfaces;

public interface IProductionLineService
{
    Task<IEnumerable<ProductionLineDto>> GetAllAsync();
    Task<ProductionLineDto?> GetByIdAsync(long id);
    Task<IEnumerable<ProductionLineDto>> GetByWorkshopIdAsync(long workshopId);
    Task<ProductionLineDto> CreateAsync(Domain.Entities.ProductionLine entity);
    Task UpdateAsync(long id, Domain.Entities.ProductionLine entity);
    Task DeleteAsync(long id);
}
