using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MES.Application.Dtos;
using MES.Application.Interfaces;
using MES.Infrastructure.Data;

namespace MES.Infrastructure.Services;

public class SeedService : ISeedService
{
    private readonly MesDbContext _db;
    private readonly ILogger<SeedService> _logger;

    public SeedService(MesDbContext db, ILogger<SeedService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<SeedResultDto> InitializeAsync()
    {
        await _db.Database.MigrateAsync();

        var result = new SeedResultDto
        {
            FactoryCount = await _db.Factories.CountAsync(),
            WorkshopCount = await _db.Workshops.CountAsync(),
            LineCount = await _db.ProductionLines.CountAsync(),
            WorkstationCount = await _db.Workstations.CountAsync(),
            MaterialCount = await _db.Materials.CountAsync(),
            BomCount = await _db.Boms.CountAsync(),
            RoutingCount = await _db.Routings.CountAsync(),
            WorkOrderCount = await _db.WorkOrders.CountAsync(),
            UserCount = await _db.Users.CountAsync()
        };

        _logger.LogInformation("种子数据初始化完成: {@Result}", result);
        return result;
    }
}
