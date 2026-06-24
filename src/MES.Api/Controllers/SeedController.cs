using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MES.Application.Interfaces;
using MES.Api.Middleware;

namespace MES.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize(Roles = "admin")]
public class SeedController : ControllerBase
{
    private readonly ISeedService _seedService;

    public SeedController(ISeedService seedService) => _seedService = seedService;

    /// <summary>
    /// 执行种子数据初始化。
    /// 所有操作在一个事务中完成，如果数据已存在则跳过（幂等）。
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Seed()
    {
        var stats = await _seedService.SeedAsync();
        return Ok(new ApiResponse(0, "种子数据创建完成", stats));
    }
}
