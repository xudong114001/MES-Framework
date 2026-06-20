using System.Linq.Expressions;
using MES.Domain.Entities;

namespace MES.Domain.Repositories;

public interface IMaterialRepository
{
    Task<Material?> GetByIdAsync(long id);
    Task<IEnumerable<Material>> GetAllAsync();
    Task<Material> AddAsync(Material entity);
    Task UpdateAsync(Material entity);
    Task DeleteAsync(Material entity);
    Task<IEnumerable<Material>> FindAsync(Expression<Func<Material, bool>> predicate);
}
