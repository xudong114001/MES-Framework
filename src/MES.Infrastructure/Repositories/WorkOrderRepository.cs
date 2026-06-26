using MES.Domain.Entities;
using MES.Domain.Repositories;
using MES.Infrastructure.Data;

namespace MES.Infrastructure.Repositories;

public class WorkOrderRepository : Repository<WorkOrder>, IWorkOrderRepository
{
    public WorkOrderRepository(MesDbContext context) : base(context) { }
}
