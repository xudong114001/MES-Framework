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
