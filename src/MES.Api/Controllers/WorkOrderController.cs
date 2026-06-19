using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MES.Api.Middleware;
using MES.Application.Interfaces;
using MES.Domain.Entities;
using MES.Infrastructure.Repositories;

namespace MES.Api.Controllers;

[ApiController]
[Route("api/v1/work-orders")]
[Authorize(Roles = "admin,supervisor,operator")]
public class WorkOrderController : ControllerBase
{
    private readonly IRepository<WorkOrder> _repo;
    private readonly IWorkOrderService _service;

    public WorkOrderController(IRepository<WorkOrder> repo, IWorkOrderService service)
    {
        _repo = repo;
        _service = service;
    }

    /// <summary>
    /// 获取所有工单列表
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var list = await _repo.GetAllAsync();
        return Ok(ApiResponse.Ok(list));
    }

    /// <summary>
    /// 根据ID获取工单
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(long id)
    {
        var entity = await _repo.GetByIdAsync(id);
        if (entity == null) return NotFound(ApiResponse.Fail("工单不存在"));
        return Ok(ApiResponse.Ok(entity));
    }

    /// <summary>
    /// 创建工单
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] WorkOrder entity)
    {
        try
        {
            var created = await _service.CreateWorkOrderAsync(entity);
            return Ok(ApiResponse.Ok(created));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }

    /// <summary>
    /// 更新工单
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(long id, [FromBody] WorkOrder entity)
    {
        entity.Id = id;
        await _repo.UpdateAsync(entity);
        return Ok(ApiResponse.Ok("更新成功"));
    }

    /// <summary>
    /// 删除工单
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(long id)
    {
        var entity = await _repo.GetByIdAsync(id);
        if (entity == null) return NotFound(ApiResponse.Fail("工单不存在"));
        await _repo.DeleteAsync(entity);
        return Ok(ApiResponse.Ok("删除成功"));
    }

    /// <summary>
    /// 下达工单
    /// </summary>
    [HttpPost("{id}/release")]
    public async Task<IActionResult> Release(long id)
    {
        try
        {
            await _service.ReleaseWorkOrderAsync(id);
            return Ok(ApiResponse.Ok("下达成功"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }

    /// <summary>
    /// 暂停工单
    /// </summary>
    [HttpPost("{id}/hold")]
    public async Task<IActionResult> Hold(long id)
    {
        try
        {
            await _service.HoldWorkOrderAsync(id);
            return Ok(ApiResponse.Ok("已暂停"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }

    /// <summary>
    /// 恢复工单
    /// </summary>
    [HttpPost("{id}/resume")]
    public async Task<IActionResult> Resume(long id)
    {
        try
        {
            await _service.ResumeWorkOrderAsync(id);
            return Ok(ApiResponse.Ok("已恢复"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }

    /// <summary>
    /// 取消工单
    /// </summary>
    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> Cancel(long id)
    {
        try
        {
            await _service.CancelWorkOrderAsync(id);
            return Ok(ApiResponse.Ok("已取消"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }

    /// <summary>
    /// 关闭工单
    /// </summary>
    [HttpPost("{id}/close")]
    public async Task<IActionResult> Close(long id)
    {
        try
        {
            await _service.CloseWorkOrderAsync(id);
            return Ok(ApiResponse.Ok("已关闭"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }

    /// <summary>
    /// 拆单
    /// </summary>
    [HttpPost("{id}/split")]
    public async Task<IActionResult> Split(long id, [FromBody] SplitRequest request)
    {
        try
        {
            var child = await _service.SplitWorkOrderAsync(id, request.SplitQty);
            return Ok(ApiResponse.Ok(child));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }

    /// <summary>
    /// 返工
    /// </summary>
    [HttpPost("{id}/rework")]
    public async Task<IActionResult> Rework(long id, [FromBody] ReworkRequest request)
    {
        try
        {
            var child = await _service.ReworkWorkOrderAsync(id, request.ReworkQty, request.Remark);
            return Ok(ApiResponse.Ok(child));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }

    /// <summary>
    /// 报废
    /// </summary>
    [HttpPost("{id}/scrap")]
    public async Task<IActionResult> Scrap(long id, [FromBody] ScrapRequest request)
    {
        try
        {
            await _service.ScrapWorkOrderAsync(id, request.ScrapQty, request.Remark);
            return Ok(ApiResponse.Ok("报废成功"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }
}

public class SplitRequest
{
    public decimal SplitQty { get; set; }
}

public class ReworkRequest
{
    public decimal ReworkQty { get; set; }
    public string? Remark { get; set; }
}

public class ScrapRequest
{
    public decimal ScrapQty { get; set; }
    public string? Remark { get; set; }
}
