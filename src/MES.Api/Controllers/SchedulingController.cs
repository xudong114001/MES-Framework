using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MES.Api.Middleware;
using MES.Application.Interfaces;
using MES.Domain.Entities;
using MES.Domain.Repositories;

namespace MES.Api.Controllers;

[ApiController]
[Route("api/v1/scheduling")]
[Authorize(Roles = "admin,supervisor")]
public class SchedulingController : ControllerBase
{
    private readonly ISchedulingService _schedulingService;
    private readonly IRepository<ProductionLine> _lineRepo1;

    public SchedulingController(
        ISchedulingService schedulingService,
        IRepository<ProductionLine> lineRepo1)
    {
        _schedulingService = schedulingService;
        _lineRepo1 = lineRepo1;
    }

    /// <summary>获取所有可排产工单（RELEASED 且未排产）</summary>
    [HttpGet("unscheduled-orders")]
    public async Task<IActionResult> GetUnscheduledOrders()
    {
        var list = await _schedulingService.GetUnscheduledOrdersAsync();
        return Ok(ApiResponse.Ok(list));
    }

    /// <summary>排产：将工单分配到指定产线</summary>
    [HttpPost("schedule")]
    public async Task<IActionResult> Schedule([FromBody] ScheduleRequest request)
    {
        try
        {
            await _schedulingService.ScheduleOrderAsync(request.WorkOrderId, request.LineId);
            return Ok(ApiResponse.Ok("排产成功"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }

    /// <summary>批量排产</summary>
    [HttpPost("batch-schedule")]
    public async Task<IActionResult> BatchSchedule([FromBody] BatchScheduleRequest request)
    {
        try
        {
            await _schedulingService.ScheduleOrdersAsync(request.WorkOrderIds, request.LineId);
            return Ok(ApiResponse.Ok("批量排产成功"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }

    /// <summary>自动排产</summary>
    [HttpPost("auto-schedule")]
    public async Task<IActionResult> AutoSchedule()
    {
        try
        {
            await _schedulingService.AutoScheduleAsync();
            return Ok(ApiResponse.Ok("自动排产成功"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }

    /// <summary>获取指定产线的已排产工单</summary>
    [HttpGet("line/{lineId}/scheduled-orders")]
    public async Task<IActionResult> GetLineScheduledOrders(long lineId)
    {
        var list = await _schedulingService.GetScheduledOrdersByLineAsync(lineId);
        return Ok(ApiResponse.Ok(list));
    }

    /// <summary>取消排产</summary>
    [HttpPost("unschedule/{workOrderId}")]
    public async Task<IActionResult> Unschedule(long workOrderId)
    {
        try
        {
            await _schedulingService.UnscheduleOrderAsync(workOrderId);
            return Ok(ApiResponse.Ok("已取消排产"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }

    /// <summary>交换排产顺序</summary>
    [HttpPost("swap-order")]
    public async Task<IActionResult> SwapOrder([FromBody] SwapOrderRequest request)
    {
        try
        {
            await _schedulingService.SwapSchedulingOrderAsync(request.OrderId1, request.OrderId2);
            return Ok(ApiResponse.Ok("排序调整成功"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }

    /// <summary>获取所有产线列表（供排产页面下拉选择）</summary>
    [HttpGet("production-lines")]
    public async Task<IActionResult> GetAllLines()
    {
        var lines = await _lineRepo1.FindAsync(l => l.Status);
        return Ok(ApiResponse.Ok(lines));
    }
}

public class ScheduleRequest
{
    public long WorkOrderId { get; set; }
    public long LineId { get; set; }
}

public class BatchScheduleRequest
{
    public List<long> WorkOrderIds { get; set; } = new();
    public long LineId { get; set; }
}

public class SwapOrderRequest
{
    public long OrderId1 { get; set; }
    public long OrderId2 { get; set; }
}
