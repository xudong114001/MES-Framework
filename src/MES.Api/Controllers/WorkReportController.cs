using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MES.Api.Middleware;
using MES.Api.Services;
using MES.Application.Dtos;
using MES.Application.Interfaces;

namespace MES.Api.Controllers;

[ApiController]
[Route("api/v1/work-reports")]
[Authorize(Roles = "Admin,ProductionManager,Operator")]
public class WorkReportController : ControllerBase
{
    private readonly IWorkReportService _reportService;

    public WorkReportController(IWorkReportService reportService)
    {
        _reportService = reportService;
    }

    /// <summary>
    /// 获取报工列表
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var list = await _reportService.GetAllAsync();
        return Ok(ApiResponse.Ok(list));
    }

    /// <summary>
    /// 获取报工详情
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(long id)
    {
        var entity = await _reportService.GetByIdAsync(id);
        if (entity == null)
            return NotFound(ApiResponse.Fail("报工记录不存在"));
        return Ok(ApiResponse.Ok(entity));
    }

    /// <summary>
    /// 提交报工
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Submit([FromBody] SubmitWorkReportRequest request)
    {
        var created = await _reportService.SubmitAsync(request);
        return Ok(ApiResponse.Ok(created));
    }

    /// <summary>
    /// 修改报工
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateWorkReportRequest request)
    {
        await _reportService.UpdateAsync(id, request);
        return Ok(ApiResponse.Ok("更新成功"));
    }

    /// <summary>
    /// PDA 扫码报工
    /// </summary>
    [HttpPost("pda-scan")]
    public async Task<IActionResult> PdaScan([FromBody] PdaScanReportRequest request)
    {
        var report = await _reportService.PdaScanReportAsync(request);
        return Ok(ApiResponse.Ok(report));
    }

    /// <summary>
    /// 删除报工记录（软删除，仅 Admin）
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(long id)
    {
        await _reportService.DeleteAsync(id);
        return Ok(ApiResponse.Ok("删除成功"));
    }
}
