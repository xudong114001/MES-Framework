using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MES.Api.Middleware;
using MES.Api.Services;
using MES.Application.Services;
using MES.Domain.Entities;
using MES.Infrastructure.Repositories;

namespace MES.Api.Controllers;

[ApiController]
[Route("api/v1/work-reports")]
[Authorize(Roles = "admin,supervisor,operator")]
public class WorkReportController : ControllerBase
{
    private readonly IRepository<WorkReport> _reportRepo;
    private readonly WorkReportService _reportService;
    private readonly HubNotificationService _hubNotification;

    public WorkReportController(
        IRepository<WorkReport> reportRepo,
        WorkReportService reportService,
        HubNotificationService hubNotification)
    {
        _reportRepo = reportRepo;
        _reportService = reportService;
        _hubNotification = hubNotification;
    }

    /// <summary>
    /// 获取报工列表
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var list = await _reportRepo.GetAllAsync();
        return Ok(ApiResponse.Ok(list));
    }

    /// <summary>
    /// 获取报工详情
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(long id)
    {
        var entity = await _reportRepo.GetByIdAsync(id);
        if (entity == null)
            return NotFound(ApiResponse.Fail("报工记录不存在"));
        return Ok(ApiResponse.Ok(entity));
    }

    /// <summary>
    /// 提交报工
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Submit([FromBody] WorkReport report)
    {
        try
        {
            var created = await _reportService.SubmitReportAsync(report);

            // 报工完成后推送实时更新
            _ = Task.Run(async () =>
            {
                try
                {
                    await _hubNotification.NotifyOutputUpdate(new
                    {
                        workOrderId = created.WorkOrderId,
                        goodQty = created.GoodQty,
                        scrapQty = created.ScrapQty,
                        batchNo = created.BatchNo,
                        timestamp = DateTime.UtcNow
                    });
                    await _hubNotification.NotifyOrderUpdate(new
                    {
                        workOrderId = created.WorkOrderId,
                        action = "report_submitted",
                        timestamp = DateTime.UtcNow
                    });
                }
                catch { /* SignalR push failure is non-critical */ }
            });

            return Ok(ApiResponse.Ok(created));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }

    /// <summary>
    /// 修改报工
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(long id, [FromBody] WorkReport entity)
    {
        var existing = await _reportRepo.GetByIdAsync(id);
        if (existing == null)
            return NotFound(ApiResponse.Fail("报工记录不存在"));

        entity.Id = id;
        await _reportRepo.UpdateAsync(entity);
        return Ok(ApiResponse.Ok("更新成功"));
    }

    /// <summary>
    /// PDA 扫码报工
    /// </summary>
    [HttpPost("pda-scan")]
    [AllowAnonymous]
    public async Task<IActionResult> PdaScan([FromBody] PdaScanReportRequest request)
    {
        try
        {
            var report = await _reportService.PdaScanReportAsync(request);

            // PDA 报工完成后推送实时更新
            _ = Task.Run(async () =>
            {
                try
                {
                    await _hubNotification.NotifyOutputUpdate(new
                    {
                        workOrderId = report.WorkOrderId,
                        goodQty = report.GoodQty,
                        scrapQty = report.ScrapQty,
                        batchNo = report.BatchNo,
                        timestamp = DateTime.UtcNow
                    });
                }
                catch { }
            });

            return Ok(ApiResponse.Ok(report));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }
}
