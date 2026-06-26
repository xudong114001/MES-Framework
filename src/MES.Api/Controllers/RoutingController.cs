using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MES.Api.Middleware;
using MES.Application.Dtos;
using MES.Application.Interfaces;

namespace MES.Api.Controllers;

[ApiController]
[Route("api/v1/routings")]
[Authorize(Roles = "Admin,ProductionManager")]
public class RoutingController : ControllerBase
{
    private readonly IRoutingService _service;
    public RoutingController(IRoutingService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var list = await _service.GetAllAsync();
        return Ok(ApiResponse.Ok(list));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(long id)
    {
        var result = await _service.GetByIdAsync(id);
        if (result == null) return NotFound(ApiResponse.Fail("工艺路线不存在"));
        return Ok(ApiResponse.Ok(result));
    }

    [HttpGet("by-material/{materialId}")]
    public async Task<IActionResult> GetByMaterial(long materialId)
    {
        var list = await _service.GetByMaterialIdAsync(materialId);
        return Ok(ApiResponse.Ok(list));
    }

    /// <summary>
    /// 获取工艺路线的工序步骤列表
    /// </summary>
    [HttpGet("{id}/steps")]
    public async Task<IActionResult> GetSteps(long id)
    {
        var routing = await _service.GetByIdAsync(id);
        if (routing == null) return NotFound(ApiResponse.Fail("工艺路线不存在"));
        return Ok(ApiResponse.Ok(routing.Steps));
    }

    /// <summary>
    /// 添加工序步骤
    /// </summary>
    [HttpPost("{id}/steps")]
    public async Task<IActionResult> AddStep(long id, [FromBody] AddRoutingStepRequest request)
    {
        var created = await _service.AddStepAsync(id, request);
        return Ok(ApiResponse.Ok(created));
    }

    /// <summary>
    /// 更新工序步骤
    /// </summary>
    [HttpPut("{id}/steps/{stepId}")]
    public async Task<IActionResult> UpdateStep(long id, long stepId, [FromBody] UpdateRoutingStepRequest request)
    {
        await _service.UpdateStepAsync(id, stepId, request);
        return Ok(ApiResponse.Ok("更新成功"));
    }

    /// <summary>
    /// 删除工序步骤
    /// </summary>
    [HttpDelete("{id}/steps/{stepId}")]
    public async Task<IActionResult> DeleteStep(long id, long stepId)
    {
        await _service.DeleteStepAsync(id, stepId);
        return Ok(ApiResponse.Ok("删除成功"));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRoutingRequest request)
    {
        var created = await _service.CreateAsync(request);
        return Ok(ApiResponse.Ok(created));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateRoutingRequest request)
    {
        await _service.UpdateAsync(id, request);
        return Ok(ApiResponse.Ok("更新成功"));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(long id)
    {
        await _service.DeleteAsync(id);
        return Ok(ApiResponse.Ok("删除成功"));
    }
}
