using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MES.Api.Middleware;
using MES.Application.Dtos;
using MES.Application.Interfaces;
using MES.Domain.Enums;

namespace MES.Api.Controllers;

[ApiController]
[Route("api/v1/work-orders")]
[Authorize(Roles = "Admin,ProductionManager,Operator")]
public class WorkOrderController : ControllerBase
{
    private readonly IWorkOrderService _service;

    public WorkOrderController(IWorkOrderService service)
    {
        _service = service;
    }

    /// <summary>
    /// 获取所有工单列表（支持按状态和产线过滤）
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] WorkOrderStatus? status, [FromQuery] long? lineId)
    {
        var list = await _service.GetAllAsync(status, lineId);
        return Ok(ApiResponse.Ok(list));
    }

    /// <summary>
    /// 根据ID获取工单
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(long id)
    {
        var entity = await _service.GetByIdAsync(id);
        if (entity == null) return NotFound(ApiResponse.Fail("工单不存在"));
        return Ok(ApiResponse.Ok(entity));
    }

    /// <summary>
    /// 创建工单
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateWorkOrderRequest request)
    {
        var created = await _service.CreateAsync(request);
        return Ok(ApiResponse.Ok(created));
    }

    /// <summary>
    /// 更新工单
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateWorkOrderRequest request)
    {
        await _service.UpdateAsync(id, request);
        return Ok(ApiResponse.Ok("更新成功"));
    }

    /// <summary>
    /// 删除工单
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(long id)
    {
        await _service.DeleteWorkOrderAsync(id);
        return Ok(ApiResponse.Ok("删除成功"));
    }

    /// <summary>
    /// 下达工单
    /// </summary>
    [HttpPost("{id}/release")]
    public async Task<IActionResult> Release(long id)
    {
        await _service.ReleaseWorkOrderAsync(id);
        return Ok(ApiResponse.Ok("下达成功"));
    }

    /// <summary>
    /// 暂停工单
    /// </summary>
    [HttpPost("{id}/hold")]
    public async Task<IActionResult> Hold(long id)
    {
        await _service.HoldWorkOrderAsync(id);
        return Ok(ApiResponse.Ok("已暂停"));
    }

    /// <summary>
    /// 恢复工单
    /// </summary>
    [HttpPost("{id}/resume")]
    public async Task<IActionResult> Resume(long id)
    {
        await _service.ResumeWorkOrderAsync(id);
        return Ok(ApiResponse.Ok("已恢复"));
    }

    /// <summary>
    /// 取消工单
    /// </summary>
    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> Cancel(long id)
    {
        await _service.CancelWorkOrderAsync(id);
        return Ok(ApiResponse.Ok("已取消"));
    }

    /// <summary>
    /// 关闭工单
    /// </summary>
    [HttpPost("{id}/close")]
    public async Task<IActionResult> Close(long id)
    {
        await _service.CloseWorkOrderAsync(id);
        return Ok(ApiResponse.Ok("已关闭"));
    }

    /// <summary>
    /// 拆单
    /// </summary>
    [HttpPost("{id}/split")]
    public async Task<IActionResult> Split(long id, [FromBody] SplitRequest request)
    {
        var child = await _service.SplitWorkOrderAsync(id, request.SplitQty);
        return Ok(ApiResponse.Ok(child));
    }

    /// <summary>
    /// 返工
    /// </summary>
    [HttpPost("{id}/rework")]
    public async Task<IActionResult> Rework(long id, [FromBody] ReworkRequest request)
    {
        var child = await _service.ReworkWorkOrderAsync(id, request.ReworkQty, request.Remark);
        return Ok(ApiResponse.Ok(child));
    }

    /// <summary>
    /// 报废
    /// </summary>
    [HttpPost("{id}/scrap")]
    public async Task<IActionResult> Scrap(long id, [FromBody] ScrapRequest request)
    {
        await _service.ScrapWorkOrderAsync(id, request.ScrapQty, request.Remark);
        return Ok(ApiResponse.Ok("报废成功"));
    }
}
