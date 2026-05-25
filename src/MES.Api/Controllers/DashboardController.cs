using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MES.Api.Middleware;
using MES.Application.Services;

namespace MES.Api.Controllers;

[ApiController]
[Route("api/v1/dashboard")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly DashboardService _service;

    public DashboardController(DashboardService service) => _service = service;

    /// <summary>今日工单统计</summary>
    [HttpGet("orders/today")]
    public async Task<IActionResult> TodayOrders()
        => Ok(ApiResponse.Ok(await _service.GetTodayOrderStatsAsync()));

    /// <summary>工单状态分布</summary>
    [HttpGet("orders/status")]
    public async Task<IActionResult> OrderStatus()
        => Ok(ApiResponse.Ok(await _service.GetOrderStatusDistributionAsync()));

    /// <summary>产量统计</summary>
    [HttpGet("output")]
    public async Task<IActionResult> Output()
        => Ok(ApiResponse.Ok(await _service.GetOutputStatsAsync()));

    /// <summary>设备状态</summary>
    [HttpGet("equipment")]
    public async Task<IActionResult> Equipment()
        => Ok(ApiResponse.Ok(await _service.GetEquipmentStatusAsync()));
}
