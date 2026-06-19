using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MES.AI.Application.Dtos;
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
    private readonly IKnowledgeBaseService _knowledgeBaseService;
    private readonly ILogger<AiController> _logger;

    public AiController(
        IQualityAlertService qualityAlertService,
        ISchedulingRecommendationService schedulingService,
        IEquipmentHealthService equipmentHealthService,
        IKnowledgeBaseService knowledgeBaseService,
        ILogger<AiController> logger)
    {
        _qualityAlertService = qualityAlertService;
        _schedulingService = schedulingService;
        _equipmentHealthService = equipmentHealthService;
        _knowledgeBaseService = knowledgeBaseService;
        _logger = logger;
    }

    /// <summary>
    /// 获取活跃的质量异常告警
    /// </summary>
    [HttpGet("quality/alerts")]
    public async Task<IActionResult> GetActiveAlerts()
    {
        var alerts = await _qualityAlertService.GetActiveAlertsAsync();
        return Ok(ApiResponse.Ok(alerts));
    }

    /// <summary>
    /// 分析质量数据（AI 分析）
    /// </summary>
    [HttpPost("quality/analyze")]
    public async Task<IActionResult> AnalyzeQuality([FromBody] long? workOrderId)
    {
        var alerts = await _qualityAlertService.AnalyzeAsync(workOrderId);
        return Ok(ApiResponse.Ok(alerts));
    }

    /// <summary>
    /// 处理质量告警
    /// </summary>
    [HttpPost("quality/alerts/{id}/process")]
    public async Task<IActionResult> ProcessAlert(long id, [FromBody] ProcessAlertRequest request)
    {
        await _qualityAlertService.MarkAsProcessedAsync(id, request.ProcessedBy);
        return Ok(ApiResponse.Ok("已处理"));
    }

    /// <summary>
    /// 获取质量告警历史
    /// </summary>
    [HttpGet("quality/history")]
    public async Task<IActionResult> GetAlertHistory([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var history = await _qualityAlertService.GetAlertHistoryAsync(page, pageSize);
        return Ok(ApiResponse.Ok(history));
    }

    /// <summary>
    /// 获取排产推荐
    /// </summary>
    [HttpGet("scheduling/recommend/{workOrderId}")]
    public async Task<IActionResult> GetSchedulingRecommendation(long workOrderId)
    {
        var recommendations = await _schedulingService.GetRecommendationsAsync(workOrderId);
        return Ok(ApiResponse.Ok(recommendations));
    }

    /// <summary>
    /// 获取所有设备健康状态
    /// </summary>
    [HttpGet("equipment/health")]
    public async Task<IActionResult> GetAllEquipmentHealth()
    {
        var health = await _equipmentHealthService.GetAllEquipmentHealthAsync();
        return Ok(ApiResponse.Ok(health));
    }

    /// <summary>
    /// 获取指定设备健康状态
    /// </summary>
    [HttpGet("equipment/health/{equipmentId}")]
    public async Task<IActionResult> GetEquipmentHealth(long equipmentId)
    {
        var health = await _equipmentHealthService.AnalyzeEquipmentAsync(equipmentId);
        return Ok(ApiResponse.Ok(health));
    }

    /// <summary>
    /// 获取高风险设备列表
    /// </summary>
    [HttpGet("equipment/high-risk")]
    public async Task<IActionResult> GetHighRiskEquipment()
    {
        var equipment = await _equipmentHealthService.GetHighRiskEquipmentAsync();
        return Ok(ApiResponse.Ok(equipment));
    }

    /// <summary>
    /// 搜索知识库
    /// </summary>
    [HttpGet("knowledge/search")]
    public async Task<IActionResult> SearchKnowledge([FromQuery] string? q, [FromQuery] int? category, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _knowledgeBaseService.SearchAsync(q, category, page, pageSize);
        return Ok(ApiResponse.Ok(result));
    }

    /// <summary>
    /// 获取知识库条目列表
    /// </summary>
    [HttpGet("knowledge/entries")]
    public async Task<IActionResult> GetKnowledgeEntries([FromQuery] int? category, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var entries = await _knowledgeBaseService.GetAllAsync(category, page, pageSize);
        return Ok(ApiResponse.Ok(entries));
    }

    /// <summary>
    /// 添加知识库条目
    /// </summary>
    [HttpPost("knowledge/entries")]
    public async Task<IActionResult> AddKnowledgeEntry([FromBody] KnowledgeEntryDto dto)
    {
        var entry = await _knowledgeBaseService.AddAsync(dto);
        return Ok(ApiResponse.Ok(entry));
    }

    /// <summary>
    /// 更新知识库条目
    /// </summary>
    [HttpPut("knowledge/entries/{id}")]
    public async Task<IActionResult> UpdateKnowledgeEntry(long id, [FromBody] KnowledgeEntryDto dto)
    {
        var entry = await _knowledgeBaseService.UpdateAsync(id, dto);
        if (entry == null)
            return Ok(ApiResponse.Fail("知识条目不存在"));
        return Ok(ApiResponse.Ok(entry));
    }

    /// <summary>
    /// 删除知识库条目
    /// </summary>
    [HttpDelete("knowledge/entries/{id}")]
    public async Task<IActionResult> DeleteKnowledgeEntry(long id)
    {
        var success = await _knowledgeBaseService.DeleteAsync(id);
        if (!success)
            return Ok(ApiResponse.Fail("知识条目不存在"));
        return Ok(ApiResponse.Ok("已删除"));
    }
}
