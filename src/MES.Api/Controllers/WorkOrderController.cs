using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MES.Api.Middleware;
using MES.Application.Interfaces;
using MES.Domain.Entities;
using MES.Infrastructure.Repositories;

namespace MES.Api.Controllers;

[ApiController]
[Route("api/v1/work-orders")]
[Authorize]
public class WorkOrderController : ControllerBase
{
    private readonly IRepository<WorkOrder> _repo;
    private readonly IWorkOrderService _service;

    public WorkOrderController(IRepository<WorkOrder> repo, IWorkOrderService service)
    {
        _repo = repo;
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var list = await _repo.GetAllAsync();
        return Ok(ApiResponse.Ok(list));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(long id)
    {
        var entity = await _repo.GetByIdAsync(id);
        if (entity == null) return NotFound(ApiResponse.Fail("工单不存在"));
        return Ok(ApiResponse.Ok(entity));
    }

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

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(long id, [FromBody] WorkOrder entity)
    {
        entity.Id = id;
        await _repo.UpdateAsync(entity);
        return Ok(ApiResponse.Ok("更新成功"));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(long id)
    {
        var entity = await _repo.GetByIdAsync(id);
        if (entity == null) return NotFound(ApiResponse.Fail("工单不存在"));
        await _repo.DeleteAsync(entity);
        return Ok(ApiResponse.Ok("删除成功"));
    }

    // ---- 工单流转 ----

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
