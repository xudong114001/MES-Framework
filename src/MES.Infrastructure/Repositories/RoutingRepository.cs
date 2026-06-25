using Microsoft.EntityFrameworkCore;
using MES.Domain.Entities;
using MES.Domain.Repositories;
using MES.Infrastructure.Data;

namespace MES.Infrastructure.Repositories;

public class RoutingRepository : Repository<Routing>, IRoutingRepository
{
    public RoutingRepository(MesDbContext context) : base(context) { }

    public async Task<IEnumerable<Routing>> GetAllWithStepsAsync()
    {
        return await _dbSet.Include(r => r.Steps).ToListAsync();
    }

    public async Task<Routing?> GetByIdWithStepsAsync(long id)
    {
        return await _dbSet.Include(r => r.Steps).FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<IEnumerable<Routing>> GetByMaterialIdWithStepsAsync(long materialId)
    {
        return await _dbSet
            .Include(r => r.Steps)
            .Where(r => r.MaterialId == materialId)
            .ToListAsync();
    }
}
