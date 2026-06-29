using MES.Application.Dtos;

namespace MES.Application.Interfaces;

public interface IProductionLineService
{
    Task<IEnumerable<ProductionLineDto>> GetAllAsync();
    Task<IEnumerable<ProductionLineDto>> GetAllLinesAsync();
    Task<ProductionLineDto?> GetByIdAsync(long id);
    Task<IEnumerable<ProductionLineDto>> GetByWorkshopIdAsync(long workshopId);

    /// <summary>创建生产线（DTO版本）</summary>
    Task<ProductionLineDto> CreateAsync(CreateProductionLineRequest request);

    /// <summary>更新生产线（DTO版本）</summary>
    Task UpdateAsync(long id, UpdateProductionLineRequest request);

    Task DeleteAsync(long id);
}
