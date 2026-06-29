using MES.Application.Dtos;

namespace MES.Application.Interfaces;

public interface ISeedService
{
    /// <summary>
    /// 初始化种子数据，返回各实体数量统计
    /// </summary>
    Task<SeedResultDto> InitializeAsync();
}
