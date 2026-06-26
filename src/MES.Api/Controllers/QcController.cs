using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MES.Api.Middleware;
using MES.Application.Dtos;
using MES.Application.Interfaces;
using MES.Domain.Enums;

namespace MES.Api.Controllers;

[ApiController]
[Route("api/v1/qc")]
[Authorize(Roles = "Admin,ProductionManager,QualityEngineer")]
public class QcController : ControllerBase
{
    private readonly IQcService _qcService;

    public QcController(IQcService qcService)
    {
        _qcService = qcService;
    }

    /// <summary>
    /// 获取质检单列表
    /// </summary>
    [HttpGet("inspections")]
    public async Task<IActionResult> GetInspections()
    {
        var list = await _qcService.GetAllInspectionsAsync();
        return Ok(ApiResponse.Ok(list));
    }

    /// <summary>
    /// 获取质检单详情（含质检项）
    /// </summary>
    [HttpGet("inspections/{id}")]
    public async Task<IActionResult> GetInspectionById(long id)
    {
        var result = await _qcService.GetInspectionWithItemsAsync(id);
        if (result == null)
            return NotFound(ApiResponse.Fail("质检单不存在"));

        return Ok(ApiResponse.Ok(result));
    }

    /// <summary>
    /// 创建质检单
    /// </summary>
    [HttpPost("inspections")]
    public async Task<IActionResult> CreateInspection([FromBody] CreateInspectionRequest request)
    {
        var created = await _qcService.CreateInspectionAsync(
            request.InspectNo,
            request.SourceType,
            request.WorkOrderId,
            request.MaterialId,
            request.Inspector,
            request.SourceRef,
            request.Remark);
        return Ok(ApiResponse.Ok(created));
    }

    /// <summary>
    /// 添加质检项
    /// </summary>
    [HttpPost("inspections/{id}/items")]
    public async Task<IActionResult> AddItem(long id, [FromBody] AddItemRequest request)
    {
        var created = await _qcService.AddItemAsync(id, request.ItemName, request.SpecValue);
        return Ok(ApiResponse.Ok(created));
    }

    /// <summary>
    /// 判定质检结果
    /// </summary>
    [HttpPost("inspections/{id}/verify")]
    public async Task<IActionResult> VerifyInspection(long id, [FromBody] VerifyRequest request)
    {
        await _qcService.VerifyInspectionAsync(id, request.Result);
        return Ok(ApiResponse.Ok("判定成功"));
    }

    /// <summary>
    /// 不合格品处理
    /// </summary>
    [HttpPost("inspections/{id}/handle-nonconforming")]
    public async Task<IActionResult> HandleNonconforming(long id, [FromBody] HandleNonconformingRequest request)
    {
        await _qcService.HandleNonconformingAsync(id, request.Action, request.Remark);
        return Ok(ApiResponse.Ok("处理成功"));
    }

    /// <summary>
    /// 删除质检单（软删除）
    /// </summary>
    [HttpDelete("inspections/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteInspection(long id)
    {
        await _qcService.DeleteInspectionAsync(id);
        return Ok(ApiResponse.Ok("删除成功"));
    }

    /// <summary>
    /// 获取今日质检统计
    /// </summary>
    [HttpGet("dashboard/stats")]
    public async Task<IActionResult> GetDashboardStats()
    {
        var stats = await _qcService.GetDashboardStatsAsync();
        return Ok(ApiResponse.Ok(stats));
    }

    /// <summary>
    /// 获取待检列表
    /// </summary>
    [HttpGet("dashboard/pending")]
    public async Task<IActionResult> GetPendingInspections()
    {
        var pendingList = await _qcService.GetPendingInspectionsAsync();
        return Ok(ApiResponse.Ok(pendingList));
    }

    /// <summary>
    /// 获取近期不合格品列表
    /// </summary>
    [HttpGet("dashboard/recent-failed")]
    public async Task<IActionResult> GetRecentFailedInspections()
    {
        var failedList = await _qcService.GetRecentFailedInspectionsAsync();
        return Ok(ApiResponse.Ok(failedList));
    }
}

public class VerifyRequest
{
    public QcResult Result { get; set; }
}

public class HandleNonconformingRequest
{
    /// <summary>处理动作: CONCESSION(让步接收), REWORK(返工), SCRAP(报废)</summary>
    public InspectionResult Action { get; set; }
    /// <summary>处理备注</summary>
    public string? Remark { get; set; }
}

public class CreateInspectionRequest
{
    public string InspectNo { get; set; } = string.Empty;
    public QcInspectionType SourceType { get; set; }
    public long? WorkOrderId { get; set; }
    public long? MaterialId { get; set; }
    public long? Inspector { get; set; }
    public string? SourceRef { get; set; }
    public string? Remark { get; set; }
}

public class AddItemRequest
{
    public string ItemName { get; set; } = string.Empty;
    public string? SpecValue { get; set; }
}
