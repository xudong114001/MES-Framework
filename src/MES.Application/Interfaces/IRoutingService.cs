using MES.Application.Dtos;

namespace MES.Application.Interfaces;

public interface IRoutingService
{
    Task<IEnumerable<RoutingDetailDto>> GetAllAsync();
    Task<RoutingDetailDto?> GetByIdAsync(long id);
    Task<IEnumerable<RoutingDetailDto>> GetByMaterialIdAsync(long materialId);

    /// <summary>创建工艺路线（DTO版本）</summary>
    Task<RoutingDto> CreateAsync(CreateRoutingRequest request);

    /// <summary>更新工艺路线（DTO版本）</summary>
    Task UpdateAsync(long id, UpdateRoutingRequest request);

    Task DeleteAsync(long id);
}
