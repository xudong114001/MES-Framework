using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MES.Api.Middleware;
using MES.Application.Interfaces;

namespace MES.Api.Controllers;

[ApiController]
[Route("api/v1/seed")]
[Authorize(Roles = "Admin")]
public class SeedController : ControllerBase
{
    private readonly ISeedService _seedService;
    private readonly ILogger<SeedController> _logger;

    public SeedController(ISeedService seedService, ILogger<SeedController> logger)
    {
        _seedService = seedService;
        _logger = logger;
    }

    /// <summary>
    /// 初始化种子数据
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> InitSeed()
    {
        var result = await _seedService.InitializeAsync();
        return Ok(ApiResponse.Ok(result));
    }
}
