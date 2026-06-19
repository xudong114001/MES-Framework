using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MES.Api.Middleware;
using MES.Application.Interfaces;
using MES.Domain.Entities;

namespace MES.Api.Controllers;

[ApiController]
[Route("api/v1/qc-checkpoints")]
[Authorize(Roles = "admin,supervisor,inspector")]
public class QcCheckpointController : ControllerBase
{
    private readonly IQcCheckpointService _checkpointService;

    public QcCheckpointController(IQcCheckpointService checkpointService)
    {
        _checkpointService = checkpointService;
    }

    /// <summary>
    /// 获取所有质检点（兼容前端）
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var list = await _checkpointService.GetAllCheckpointsAsync();
        return Ok(ApiResponse.Ok(list));
    }

    /// <summary>
    /// 根据 ID 获取质检点（兼容前端）
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(long id)
    {
        var checkpoint = await _checkpointService.GetCheckpointByIdAsync(id);
        if (checkpoint == null)
            return Ok(ApiResponse.Fail("质检点不存在"));
        return Ok(ApiResponse.Ok(checkpoint));
    }

    /// <summary>
    /// 更新质检点（兼容前端）
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(long id, [FromBody] QcCheckpoint checkpoint)
    {
        checkpoint.Id = id;
        try
        {
            await _checkpointService.UpdateCheckpointAsync(checkpoint);
            return Ok(ApiResponse.Ok("更新成功"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }

    /// <summary>
    /// 查询某工序配置的质检点
    /// </summary>
    [HttpGet("by-step/{stepId}")]
    public async Task<IActionResult> GetByStep(long stepId)
    {
        var list = await _checkpointService.GetCheckpointsByStepAsync(stepId);
        return Ok(ApiResponse.Ok(list));
    }

    /// <summary>
    /// 配置质检点
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Configure([FromBody] QcCheckpoint checkpoint)
    {
        try
        {
            var created = await _checkpointService.ConfigureCheckpointAsync(checkpoint);
            return Ok(ApiResponse.Ok(created));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }

    /// <summary>
    /// 取消配置质检点
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Remove(long id)
    {
        try
        {
            await _checkpointService.RemoveCheckpointAsync(id);
            return Ok(ApiResponse.Ok("删除成功"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }
}
