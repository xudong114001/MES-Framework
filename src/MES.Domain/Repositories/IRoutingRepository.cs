using System.Linq.Expressions;
using MES.Domain.Entities;

namespace MES.Domain.Repositories;

public interface IRoutingRepository
{
    Task<Routing?> GetByIdAsync(long id);
    Task<IEnumerable<Routing>> GetAllAsync();
    Task<Routing> AddAsync(Routing entity);
    Task UpdateAsync(Routing entity);
    Task DeleteAsync(Routing entity);
    Task<IEnumerable<Routing>> FindAsync(Expression<Func<Routing, bool>> predicate);
}
