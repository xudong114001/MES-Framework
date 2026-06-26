using MES.Domain.Entities;
using MES.Domain.Repositories;
using MES.Infrastructure.Data;

namespace MES.Infrastructure.Repositories;

public class EquipmentRepository : Repository<Equipment>, IEquipmentRepository
{
    public EquipmentRepository(MesDbContext context) : base(context) { }
}
