using MES.Application.Dtos;

namespace MES.Application.Interfaces;

public interface IMaterialService
{
    Task<IEnumerable<MaterialDto>> GetAllAsync();
    Task<MaterialDto?> GetByIdAsync(long id);

    /// <summary>创建物料（DTO版本）</summary>
    Task<MaterialDto> CreateAsync(CreateMaterialRequest request);

    /// <summary>更新物料（DTO版本）</summary>
    Task UpdateAsync(long id, UpdateMaterialRequest request);

    Task DeleteAsync(long id);
}
