using System.Linq.Expressions;
using MES.Domain.Entities;

namespace MES.Domain.Repositories;

public interface IQcInspectionRepository
{
    Task<QcInspection?> GetByIdAsync(long id);
    Task<IEnumerable<QcInspection>> GetAllAsync();
    Task<QcInspection> AddAsync(QcInspection entity);
    Task UpdateAsync(QcInspection entity);
    Task DeleteAsync(QcInspection entity);
    Task<IEnumerable<QcInspection>> FindAsync(Expression<Func<QcInspection, bool>> predicate);
}
