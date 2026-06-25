using System.Linq.Expressions;
using MES.Domain.Entities;

namespace MES.Domain.Repositories;

public interface IRoutingRepository : IRepository<Routing>
{
    /// <summary>
    /// 获取所有工艺路线（含工序）
    /// </summary>
    Task<IEnumerable<Routing>> GetAllWithStepsAsync();

    /// <summary>
    /// 根据ID获取工艺路线（含工序）
    /// </summary>
    Task<Routing?> GetByIdWithStepsAsync(long id);

    /// <summary>
    /// 根据物料ID获取工艺路线（含工序）
    /// </summary>
    Task<IEnumerable<Routing>> GetByMaterialIdWithStepsAsync(long materialId);
}
