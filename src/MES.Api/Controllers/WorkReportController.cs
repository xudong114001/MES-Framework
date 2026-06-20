using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MES.Api.Services;
using MES.Application.Dtos;
using MES.Application.Interfaces;
using MES.Domain.Entities;

namespace MES.Api.Controllers;

[ApiController]
[Route("api/v1/work-reports")]
[Authorize(Roles = "admin,supervisor,operator")]
public class WorkReportController : BaseController
{
    private readonly IWorkReportService _reportService;
    private readonly HubNotificationService _hubNotification;

    public WorkReportController(
        IWorkReportService reportService,
        HubNotificationService hubNotification)
    {
        _reportService = reportService;
        _hubNotification = hubNotification;
    }

    /// <summary>
    /// 获取报工列表
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var list = await _reportService.GetAllAsync();
        return Success(list);
    }

    /// <summary>
    /// 获取报工详情
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(long id)
    {
        var entity = await _reportService.GetByIdAsync(id);
        if (entity == null)
            return Fail("报工记录不存在", 404);
        return Success(entity);
    }

    /// <summary>
    /// 提交报工
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Submit([FromBody] WorkReport report)
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

        return Success(created);
    }

    /// <summary>
    /// 修改报工
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(long id, [FromBody] WorkReport entity)
    {
        entity.Id = id;
        await _reportService.UpdateWorkReportAsync(entity);
        return Success("更新成功");
    }

    /// <summary>
    /// PDA 扫码报工
    /// </summary>
    [HttpPost("pda-scan")]
    [AllowAnonymous]
    public async Task<IActionResult> PdaScan([FromBody] PdaScanReportRequest request)
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

        return Success(report);
    }
}