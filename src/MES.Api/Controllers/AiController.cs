using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MES.AI.Application.Interfaces;
using MES.Api.Middleware;

namespace MES.Api.Controllers;

public class ProcessAlertRequest
{
    public string ProcessedBy { get; set; } = string.Empty;
}

[ApiController]
[Route("api/v1/ai")]
[Authorize]
public class AiController : ControllerBase
{
    private readonly IQualityAlertService _qualityAlertService;
    private readonly ISchedulingRecommendationService _schedulingService;
    private readonly IEquipmentHealthService _equipmentHealthService;
    private readonly ILogger<AiController> _logger;

    public AiController(
        IQualityAlertService qualityAlertService,
        ISchedulingRecommendationService schedulingService,
        IEquipmentHealthService equipmentHealthService,
        ILogger<AiController> logger)
    {
        _qualityAlertService = qualityAlertService;
        _schedulingService = schedulingService;
        _equipmentHealthService = equipmentHealthService;
        _logger = logger;
    }

    [HttpGet("quality/alerts")]
    public async Task<IActionResult> GetActiveAlerts()
    {
        var alerts = await _qualityAlertService.GetActiveAlertsAsync();
        return Ok(ApiResponse.Ok(alerts));
    }

    [HttpPost("quality/analyze")]
    public async Task<IActionResult> AnalyzeQuality([FromBody] long? workOrderId)
    {
        var alerts = await _qualityAlertService.AnalyzeAsync(workOrderId);
        return Ok(ApiResponse.Ok(alerts));
    }

    [HttpPost("quality/alerts/{id}/process")]
    public async Task<IActionResult> ProcessAlert(long id, [FromBody] ProcessAlertRequest request)
    {
        await _qualityAlertService.MarkAsProcessedAsync(id, request.ProcessedBy);
        return Ok(ApiResponse.Ok("已处理"));
    }

    [HttpGet("quality/history")]
    public async Task<IActionResult> GetAlertHistory([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var history = await _qualityAlertService.GetAlertHistoryAsync(page, pageSize);
        return Ok(ApiResponse.Ok(history));
    }

    [HttpGet("scheduling/recommend/{workOrderId}")]
    public async Task<IActionResult> GetSchedulingRecommendation(long workOrderId)
    {
        var recommendations = await _schedulingService.GetRecommendationsAsync(workOrderId);
        return Ok(ApiResponse.Ok(recommendations));
    }

    [HttpGet("equipment/health")]
    public async Task<IActionResult> GetAllEquipmentHealth()
    {
        var health = await _equipmentHealthService.GetAllEquipmentHealthAsync();
        return Ok(ApiResponse.Ok(health));
    }

    [HttpGet("equipment/health/{equipmentId}")]
    public async Task<IActionResult> GetEquipmentHealth(long equipmentId)
    {
        var health = await _equipmentHealthService.AnalyzeEquipmentAsync(equipmentId);
        return Ok(ApiResponse.Ok(health));
    }

    [HttpGet("equipment/high-risk")]
    public async Task<IActionResult> GetHighRiskEquipment()
    {
        var equipment = await _equipmentHealthService.GetHighRiskEquipmentAsync();
        return Ok(ApiResponse.Ok(equipment));
    }
}
