using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MES.Api.Middleware;
using MES.Api.Services;
using MES.Application.Services;
using MES.Domain.Entities;

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

    /// <summary>获取异常事件（分页）</summary>
    [HttpGet]
    public async Task<IActionResult> GetEvents(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] bool? isResolved = null,
        [FromQuery] string? eventType = null)
    {
        AndonEventType? type = null;
        if (!string.IsNullOrEmpty(eventType) && Enum.TryParse<AndonEventType>(eventType, true, out var parsed))
        {
            type = parsed;
        }

        var (items, total) = await _service.GetEventsAsync(page, pageSize, isResolved, type);
        return Ok(ApiResponse.Ok(new
        {
            Items = items.ToList(),
            Total = total,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(total / (double)pageSize)
        }));
    }

    /// <summary>获取未解决事件数量</summary>
    [HttpGet("count")]
    public async Task<IActionResult> GetActiveCount()
        => Ok(ApiResponse.Ok(new { count = await _service.GetActiveCountAsync() }));

    /// <summary>按类型统计未解决事件</summary>
    [HttpGet("count-by-type")]
    public async Task<IActionResult> GetActiveCountByType()
        => Ok(ApiResponse.Ok(await _service.GetActiveCountByTypeAsync()));

    /// <summary>触发异常事件</summary>
    [HttpPost("trigger")]
    public async Task<IActionResult> Trigger([FromBody] TriggerAndonRequest request)
    {
        if (!Enum.TryParse<AndonEventType>(request.EventType, true, out var eventType))
            return Ok(ApiResponse.Fail($"无效的事件类型: {request.EventType}"));

        if (!Enum.TryParse<AndonEventLevel>(request.Level ?? "Warning", true, out var level))
            level = AndonEventLevel.Warning;

        var userId = GetCurrentUserId();
        var userName = GetCurrentUserName();

        var evt = await _service.TriggerEventAsync(
            eventType,
            level,
            request.Title ?? $"{eventType} 异常",
            request.Description,
            request.WorkstationId,
            request.WorkstationName,
            request.WorkOrderId,
            request.WorkOrderNo,
            userId,
            userName);

        // 触发异常后推送实时通知
        _ = Task.Run(async () =>
        {
            try
            {
                await _hubNotification.NotifyAndonEvent(evt);
            }
            catch (Exception ex)
            {
                // 记录日志但不影响主流程
                Serilog.Log.Warning(ex, "Andon 事件推送失败");
            }
        });

        return Ok(ApiResponse.Ok(evt));
    }

    /// <summary>处理异常事件</summary>
    [HttpPost("{id:long}/resolve")]
    public async Task<IActionResult> Resolve(long id, [FromBody] ResolveAndonRequest request)
    {
        var userId = GetCurrentUserId();
        var userName = GetCurrentUserName() ?? "系统";

        var success = await _service.ResolveEventAsync(id, userId ?? 0, userName ?? "系统");
        if (!success)
            return Ok(ApiResponse.Fail("事件不存在或已被处理"));

        return Ok(ApiResponse.Ok(new { message = "事件已处理" }));
    }

    /// <summary>根据 ID 获取事件详情</summary>
    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(long id)
    {
        var evt = await _service.GetByIdAsync(id);
        if (evt == null)
            return Ok(ApiResponse.Fail("事件不存在"));

        return Ok(ApiResponse.Ok(evt));
    }

    /// <summary>删除异常事件（软删除）</summary>
    [HttpDelete("{id:long}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Delete(long id)
    {
        var success = await _service.DeleteEventAsync(id);
        if (!success)
            return Ok(ApiResponse.Fail("删除失败"));

        return Ok(ApiResponse.Ok(new { message = "删除成功" }));
    }

    private long? GetCurrentUserId()
    {
        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return long.TryParse(idClaim, out var id) ? id : null;
    }

    private string? GetCurrentUserName()
    {
        return User.FindFirst(ClaimTypes.Name)?.Value;
    }
}

public class TriggerAndonRequest
{
    public string EventType { get; set; } = string.Empty;
    public string? Level { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public long? WorkstationId { get; set; }
    public string? WorkstationName { get; set; }
    public long? WorkOrderId { get; set; }
    public string? WorkOrderNo { get; set; }
}

public class ResolveAndonRequest
{
    public string? Handler { get; set; }
}