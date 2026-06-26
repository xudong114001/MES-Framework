using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MES.Api.Middleware;
using MES.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MES.Api.Controllers;

[ApiController]
[Route("api/v1/seed")]
[Authorize(Roles = "Admin")]
public class SeedController : ControllerBase
{
    private readonly MesDbContext _db;
    private readonly ILogger<SeedController> _logger;

    public SeedController(MesDbContext db, ILogger<SeedController> logger)
    {
        _db = db;
        _logger = logger;
    }

    /// <summary>
    /// 初始化种子数据
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> InitSeed()
    {
        try
        {
            // 确保数据库已创建并应用种子数据
            await _db.Database.EnsureCreatedAsync();

            var result = new
            {
                factoryCount = await _db.Factories.CountAsync(),
                workshopCount = await _db.Workshops.CountAsync(),
                lineCount = await _db.ProductionLines.CountAsync(),
                workstationCount = await _db.Workstations.CountAsync(),
                materialCount = await _db.Materials.CountAsync(),
                bomCount = await _db.Boms.CountAsync(),
                routingCount = await _db.Routings.CountAsync(),
                workOrderCount = await _db.WorkOrders.CountAsync(),
                userCount = await _db.Users.CountAsync()
            };

            _logger.LogInformation("种子数据初始化完成: {@Result}", result);
            return Ok(ApiResponse.Ok(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "种子数据初始化失败");
            return Ok(ApiResponse.Fail($"初始化失败: {ex.Message}"));
        }
    }
}
