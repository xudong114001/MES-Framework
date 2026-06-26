using MES.Domain.Entities;
using MES.Domain.Repositories;
using MES.Infrastructure.Data;

namespace MES.Infrastructure.Repositories;

public class MaterialRepository : Repository<Material>, IMaterialRepository
{
    public MaterialRepository(MesDbContext context) : base(context) { }
}
