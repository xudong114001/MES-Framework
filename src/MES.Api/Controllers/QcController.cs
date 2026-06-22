using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MES.Api.Middleware;
using MES.Application.Dtos;
using MES.Application.Interfaces;
using MES.Domain.Entities;
using MES.Domain.Enums;
using MES.Domain.Exceptions;
using MES.Domain.Repositories;

namespace MES.Api.Controllers;

[ApiController]
[Route("api/v1/qc")]
[Authorize(Roles = "admin,supervisor,inspector")]
public class QcController : ControllerBase
{
    private readonly IRepository<QcInspection> _inspectionRepo;
    private readonly IRepository<QcInspectionItem> _itemRepo;
    private readonly IQcService _qcService;

    public QcController(
        IRepository<QcInspection> inspectionRepo,
        IRepository<QcInspectionItem> itemRepo,
        IQcService qcService)
    {
        _inspectionRepo = inspectionRepo;
        _itemRepo = itemRepo;
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
        var inspection = await _qcService.GetInspectionByIdAsync(id);
        if (inspection == null)
            return NotFound(ApiResponse.Fail("质检单不存在"));

        var items = await _itemRepo.FindAsync(i => i.InspectionId == id);
        return Ok(ApiResponse.Ok(new { Inspection = inspection, Items = items }));
    }

    /// <summary>
    /// 创建质检单
    /// </summary>
    [HttpPost("inspections")]
    public async Task<IActionResult> CreateInspection([FromBody] CreateInspectionRequest request)
    {
        try
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
        catch (DomainException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }

    /// <summary>
    /// 添加质检项
    /// </summary>
    [HttpPost("inspections/{id}/items")]
    public async Task<IActionResult> AddItem(long id, [FromBody] AddItemRequest request)
    {
        try
        {
            var created = await _qcService.AddItemAsync(id, request.ItemName, request.SpecValue);
            return Ok(ApiResponse.Ok(created));
        }
        catch (DomainException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }

    /// <summary>
    /// 判定质检结果
    /// </summary>
    [HttpPost("inspections/{id}/verify")]
    public async Task<IActionResult> VerifyInspection(long id, [FromBody] VerifyRequest request)
    {
        try
        {
            if (!Enum.TryParse<QcResult>(request.Result, true, out var result))
                return BadRequest(ApiResponse.Fail($"无效的质检结果: {request.Result}"));

            await _qcService.VerifyInspectionAsync(id, result);
            return Ok(ApiResponse.Ok("判定成功"));
        }
        catch (DomainException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }

    /// <summary>
    /// 不合格品处理
    /// </summary>
    [HttpPost("inspections/{id}/handle-nonconforming")]
    public async Task<IActionResult> HandleNonconforming(long id, [FromBody] HandleNonconformingRequest request)
    {
        try
        {
            await _qcService.HandleNonconformingAsync(id, request.Action, request.Remark);
            return Ok(ApiResponse.Ok("处理成功"));
        }
        catch (DomainException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }

    /// <summary>
    /// 获取今日质检统计
    /// </summary>
    [HttpGet("dashboard/stats")]
    public async Task<IActionResult> GetDashboardStats()
    {
        var todayStart = DateTime.UtcNow.Date;
        var todayEnd = todayStart.AddDays(1);

        var allInspections = await _inspectionRepo.GetAllAsync();
        var todayInspections = allInspections.Where(i => i.CreatedAt >= todayStart && i.CreatedAt < todayEnd).ToList();

        var total = todayInspections.Count;
        var passed = todayInspections.Count(i => i.InspectResult == QcResult.PASS);
        var failed = todayInspections.Count(i => i.InspectResult == QcResult.FAIL);
        var pending = todayInspections.Count(i => i.InspectResult == QcResult.PENDING);

        return Ok(ApiResponse.Ok(new
        {
            total,
            passed,
            failed,
            pending
        }));
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
    public string Result { get; set; } = string.Empty;
}

public class HandleNonconformingRequest
{
    /// <summary>处理动作: CONCESSION(让步接收), REWORK(返工), SCRAP(报废)</summary>
    public string Action { get; set; } = string.Empty;
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
