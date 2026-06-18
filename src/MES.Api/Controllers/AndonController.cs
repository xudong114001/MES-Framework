using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MES.Api.Middleware;
using MES.Api.Services;
using MES.Application.Services;

namespace MES.Api.Controllers;

[ApiController]
[Route("api/v1/andon")]
[Authorize(Roles = "admin,supervisor,viewer")]
public class AndonController : ControllerBase
{
    private readonly AndonService _service;
    private readonly HubNotificationService _hubNotification;

    public AndonController(AndonService service, HubNotificationService hubNotification)
    {
        _service = service;
        _hubNotification = hubNotification;
    }

    /// <summary>获取所有异常事件</summary>
    [HttpGet("events")]
    public async Task<IActionResult> GetAll()
        => Ok(ApiResponse.Ok(await _service.GetAllEventsAsync()));

    /// <summary>获取活跃（未处理）异常事件</summary>
    [HttpGet("active")]
    public async Task<IActionResult> GetActive()
        => Ok(ApiResponse.Ok(await _service.GetActiveEventsAsync()));

    /// <summary>触发异常事件</summary>
    [HttpPost("trigger")]
    public async Task<IActionResult> Trigger([FromBody] TriggerAndonRequest request)
    {
        if (!Enum.TryParse<AndonEventType>(request.EventType, true, out var eventType))
            return Ok(ApiResponse.Fail($"无效的事件类型: {request.EventType}"));

        var evt = await _service.TriggerEventAsync(eventType, request.Workstation, request.Description);

        // 触发异常后推送实时通知
        _ = Task.Run(async () =>
        {
            try
            {
                await _hubNotification.NotifyAndonEvent(evt);
            }
            catch { }
        });

        return Ok(ApiResponse.Ok(evt));
    }

    /// <summary>处理异常事件</summary>
    [HttpPost("{id:long}/resolve")]
    public async Task<IActionResult> Resolve(long id, [FromBody] ResolveAndonRequest request)
    {
        await _service.ResolveEventAsync(id, request.Handler);
        return Ok(ApiResponse.Ok(new { message = "事件已处理" }));
    }
}

public class TriggerAndonRequest
{
    public string EventType { get; set; } = string.Empty;
    public string? Workstation { get; set; }
    public string? Description { get; set; }
}

public class ResolveAndonRequest
{
    public string? Handler { get; set; }
}
