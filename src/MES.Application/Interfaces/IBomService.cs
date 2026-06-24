using MES.Application.Dtos;

namespace MES.Application.Interfaces;

public interface IBomService
{
    Task<IEnumerable<BomDto>> GetAllAsync();
    Task<BomDto?> GetByIdAsync(long id);
    Task<IEnumerable<BomDto>> GetByProductIdAsync(long productId);

    /// <summary>创建 BOM（DTO版本）</summary>
    Task<BomDto> CreateAsync(CreateBomRequest request);

    /// <summary>更新 BOM（DTO版本）</summary>
    Task UpdateAsync(long id, UpdateBomRequest request);

    Task DeleteAsync(long id);
}
