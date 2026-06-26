using MES.Domain.Entities;
using MES.Domain.Repositories;
using MES.Infrastructure.Data;

namespace MES.Infrastructure.Repositories;

public class QcInspectionRepository : Repository<QcInspection>, IQcInspectionRepository
{
    public QcInspectionRepository(MesDbContext context) : base(context) { }
}
