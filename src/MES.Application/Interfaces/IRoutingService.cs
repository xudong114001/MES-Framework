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

    /// <summary>添加工序步骤</summary>
    Task<RoutingStepDto> AddStepAsync(long routingId, AddRoutingStepRequest request);

    /// <summary>更新工序步骤</summary>
    Task UpdateStepAsync(long routingId, long stepId, UpdateRoutingStepRequest request);

    /// <summary>删除工序步骤</summary>
    Task DeleteStepAsync(long routingId, long stepId);
}
