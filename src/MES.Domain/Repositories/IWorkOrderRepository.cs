using System.Linq.Expressions;
using MES.Domain.Entities;

namespace MES.Domain.Repositories;

public interface IWorkOrderRepository
{
    Task<WorkOrder?> GetByIdAsync(long id);
    Task<IEnumerable<WorkOrder>> GetAllAsync();
    Task<WorkOrder> AddAsync(WorkOrder entity);
    Task UpdateAsync(WorkOrder entity);
    Task DeleteAsync(WorkOrder entity);
    Task<IEnumerable<WorkOrder>> FindAsync(Expression<Func<WorkOrder, bool>> predicate);
}
