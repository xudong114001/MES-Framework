using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MES.Api.Middleware;
using MES.Application.Interfaces;

namespace MES.Api.Controllers;

[ApiController]
[Route("api/v1/dispatch")]
[Authorize(Roles = "admin,supervisor")]
public class DispatchController : ControllerBase
{
    private readonly IDispatchService _dispatchService;

    public DispatchController(IDispatchService dispatchService)
    {
        _dispatchService = dispatchService;
    }

    /// <summary>派工：将工序分配到指定工位</summary>
    [HttpPost("dispatch-step")]
    public async Task<IActionResult> DispatchStep([FromBody] DispatchStepRequest request)
    {
        try
        {
            await _dispatchService.DispatchStepAsync(request.WorkOrderStepId, request.WorkstationId);
            return Ok(ApiResponse.Ok("派工成功"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }

    /// <summary>取消派工</summary>
    [HttpPost("undispatch-step/{workOrderStepId}")]
    public async Task<IActionResult> UndispatchStep(long workOrderStepId)
    {
        try
        {
            await _dispatchService.UndispatchStepAsync(workOrderStepId);
            return Ok(ApiResponse.Ok("已取消派工"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }

    /// <summary>查询产线今日派工任务</summary>
    [HttpGet("line/{lineId}/today-tasks")]
    public async Task<IActionResult> GetTodayTasks(long lineId)
    {
        var tasks = await _dispatchService.GetTodayDispatchedOrdersByLineAsync(lineId);
        return Ok(ApiResponse.Ok(tasks));
    }

    /// <summary>获取产线下可选工位</summary>
    [HttpGet("line/{lineId}/workstations")]
    public async Task<IActionResult> GetWorkstations(long lineId)
    {
        var list = await _dispatchService.GetAvailableWorkstationsAsync(lineId);
        return Ok(ApiResponse.Ok(list));
    }
}

public class DispatchStepRequest
{
    public long WorkOrderStepId { get; set; }
    public long WorkstationId { get; set; }
}
