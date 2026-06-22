using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MES.Application.Dtos;
using MES.Application.Interfaces;
using MES.Domain.Entities;

namespace MES.Api.Controllers;

[ApiController]
[Route("api/v1/work-orders")]
[Authorize(Roles = "admin,supervisor,operator")]
public class WorkOrderController : BaseController
{
    private readonly IWorkOrderService _service;

    public WorkOrderController(IWorkOrderService service)
    {
        _service = service;
    }

    /// <summary>
    /// 获取所有工单列表
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var list = await _service.GetAllAsync();
        return Success(list);
    }

    /// <summary>
    /// 根据ID获取工单
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(long id)
    {
        var entity = await _service.GetByIdAsync(id);
        if (entity == null) return Fail("工单不存在", 404);
        return Success(entity);
    }

    /// <summary>
    /// 创建工单
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] WorkOrder entity)
    {
        var created = await _service.CreateWorkOrderAsync(entity);
        return Success(created);
    }

    /// <summary>
    /// 更新工单
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(long id, [FromBody] WorkOrder entity)
    {
        entity.Id = id;
        await _service.UpdateWorkOrderAsync(entity);
        return Success("更新成功");
    }

    /// <summary>
    /// 删除工单
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(long id)
    {
        await _service.DeleteWorkOrderAsync(id);
        return Success("删除成功");
    }

    /// <summary>
    /// 下达工单
    /// </summary>
    [HttpPost("{id}/release")]
    public async Task<IActionResult> Release(long id)
    {
        await _service.ReleaseWorkOrderAsync(id);
        return Success("下达成功");
    }

    /// <summary>
    /// 暂停工单
    /// </summary>
    [HttpPost("{id}/hold")]
    public async Task<IActionResult> Hold(long id)
    {
        await _service.HoldWorkOrderAsync(id);
        return Success("已暂停");
    }

    /// <summary>
    /// 恢复工单
    /// </summary>
    [HttpPost("{id}/resume")]
    public async Task<IActionResult> Resume(long id)
    {
        await _service.ResumeWorkOrderAsync(id);
        return Success("已恢复");
    }

    /// <summary>
    /// 取消工单
    /// </summary>
    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> Cancel(long id)
    {
        await _service.CancelWorkOrderAsync(id);
        return Success("已取消");
    }

    /// <summary>
    /// 关闭工单
    /// </summary>
    [HttpPost("{id}/close")]
    public async Task<IActionResult> Close(long id)
    {
        await _service.CloseWorkOrderAsync(id);
        return Success("已关闭");
    }

    /// <summary>
    /// 拆单
    /// </summary>
    [HttpPost("{id}/split")]
    public async Task<IActionResult> Split(long id, [FromBody] SplitRequest request)
    {
        var child = await _service.SplitWorkOrderAsync(id, request.SplitQty);
        return Success(child);
    }

    /// <summary>
    /// 返工
    /// </summary>
    [HttpPost("{id}/rework")]
    public async Task<IActionResult> Rework(long id, [FromBody] ReworkRequest request)
    {
        var child = await _service.ReworkWorkOrderAsync(id, request.ReworkQty, request.Remark);
        return Success(child);
    }

    /// <summary>
    /// 报废
    /// </summary>
    [HttpPost("{id}/scrap")]
    public async Task<IActionResult> Scrap(long id, [FromBody] ScrapRequest request)
    {
        await _service.ScrapWorkOrderAsync(id, request.ScrapQty, request.Remark);
        return Success("报废成功");
    }
}