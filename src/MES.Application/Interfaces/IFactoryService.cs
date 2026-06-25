using MES.Application.Dtos;

namespace MES.Application.Interfaces;

public interface IFactoryService
{
    Task<IEnumerable<FactoryDto>> GetAllAsync();
    Task<FactoryDto?> GetByIdAsync(long id);

    /// <summary>创建工厂（DTO版本）</summary>
    Task<FactoryDto> CreateAsync(CreateFactoryRequest request);

    /// <summary>更新工厂（DTO版本）</summary>
    Task UpdateAsync(long id, UpdateFactoryRequest request);

    Task DeleteAsync(long id);
}
