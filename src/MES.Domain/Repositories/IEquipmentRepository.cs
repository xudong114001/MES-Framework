using System.Linq.Expressions;
using MES.Domain.Entities;

namespace MES.Domain.Repositories;

public interface IEquipmentRepository
{
    Task<Equipment?> GetByIdAsync(long id);
    Task<IEnumerable<Equipment>> GetAllAsync();
    Task<Equipment> AddAsync(Equipment entity);
    Task UpdateAsync(Equipment entity);
    Task DeleteAsync(Equipment entity);
    Task<IEnumerable<Equipment>> FindAsync(Expression<Func<Equipment, bool>> predicate);
}
