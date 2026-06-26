using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MES.Api.Middleware;
using MES.Application.Integration.Events;
using MES.Integration.Adapters;
using MES.Integration.EventBus;
using MES.Integration.Models;
using MES.Integration.Plc;

namespace MES.Api.Controllers;

/// <summary>
/// 接口集成与数据采集（P5）
/// </summary>
[ApiController]
[Route("api/v1/integration")]
[Authorize(Roles = "Admin")]
public class IntegrationController : ControllerBase
{
    private readonly IERPAdapter _erpAdapter;
    private readonly IWMSAdapter _wmsAdapter;
    private readonly IPlcCollector _plcCollector;
    private readonly IEventBus _eventBus;
    private readonly InMemoryEventLogService _eventLog;
    private readonly ILogger<IntegrationController> _logger;

    public IntegrationController(
        IERPAdapter erpAdapter,
        IWMSAdapter wmsAdapter,
        IPlcCollector plcCollector,
        IEventBus eventBus,
        InMemoryEventLogService eventLog,
        ILogger<IntegrationController> logger)
    {
        _erpAdapter = erpAdapter;
        _wmsAdapter = wmsAdapter;
        _plcCollector = plcCollector;
        _eventBus = eventBus;
        _eventLog = eventLog;
        _logger = logger;
    }

    /// <summary>
    /// 获取所有适配器状态
    /// </summary>
    [HttpGet("adapters/status")]
    public IActionResult GetAdapterStatus()
    {
        var statuses = new List<AdapterStatus>
        {
            _erpAdapter.GetStatus(),
            _wmsAdapter.GetStatus(),
            new()
            {
                Name = _plcCollector.DeviceName,
                Type = "PLC",
                IsConnected = _plcCollector.IsConnected,
                LastSyncTime = DateTime.UtcNow,
                Status = _plcCollector.IsConnected ? "Connected" : "Disconnected"
            }
        };
        return Ok(ApiResponse.Ok(statuses));
    }

    /// <summary>
    /// 获取所有适配器（兼容前端）
    /// </summary>
    [HttpGet("adapters")]
    public IActionResult GetAdapters() => GetAdapterStatus();

    /// <summary>
    /// 测试指定适配器（兼容前端）
    /// </summary>
    [HttpPost("adapters/{name}/test")]
    public async Task<IActionResult> TestAdapter(string name)
    {
        var result = name.ToLower() switch
        {
            "erp" or "sap" or "kingdee" => await _erpAdapter.TestConnectionAsync(),
            "wms" => await _wmsAdapter.TestConnectionAsync(),
            "plc" => _plcCollector.IsConnected,
            _ => false
        };
        return Ok(ApiResponse.Ok(new { connected = result, name }));
    }

    /// <summary>
    /// 同步指定适配器（兼容前端）
    /// </summary>
    [HttpPost("adapters/{name}/sync")]
    public async Task<IActionResult> SyncAdapter(string name, [FromBody] SyncAdapterRequest? request)
    {
        var direction = request?.Direction?.ToLower() ?? "pull";

        try
        {
            object? result = name.ToLower() switch
            {
                "erp" or "sap" or "kingdee" when direction == "pull" =>
                    await _erpAdapter.PullWorkOrdersAsync(null),
                "wms" when direction == "pull" =>
                    await _wmsAdapter.PullInventoryAsync(null),
                _ => null
            };
            return Ok(ApiResponse.Ok(new { success = true, data = result }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "同步适配器失败: {Name}", name);
            return Ok(ApiResponse.Fail($"同步失败: {ex.Message}"));
        }
    }

    /// <summary>
    /// 获取事件日志（兼容前端）
    /// </summary>
    [HttpGet("events")]
    public IActionResult GetEvents()
    {
        var logs = _eventLog.GetLogs();
        return Ok(ApiResponse.Ok(logs));
    }

    /// <summary>
    /// 获取事件日志（兼容后端）
    /// </summary>
    [HttpGet("event-logs")]
    public IActionResult GetEventLogs() => GetEvents();

    /// <summary>
    /// 清空事件日志（兼容前端）
    /// </summary>
    [HttpDelete("events")]
    public IActionResult ClearEvents()
    {
        _eventLog.Clear();
        return Ok(ApiResponse.Ok("已清空"));
    }

    /// <summary>
    /// 测试 ERP 连接
    /// </summary>
    [HttpGet("erp/test")]
    public async Task<IActionResult> TestErpConnection()
    {
        var result = await _erpAdapter.TestConnectionAsync();
        return Ok(ApiResponse.Ok(new { connected = result, name = _erpAdapter.AdapterName }));
    }

    /// <summary>
    /// 从 ERP 拉取工单
    /// </summary>
    [HttpPost("erp/sync-work-orders")]
    public async Task<IActionResult> SyncWorkOrders([FromBody] SyncRequest? request)
    {
        var result = await _erpAdapter.PullWorkOrdersAsync(request?.Since);
        return Ok(ApiResponse.Ok(result));
    }

    /// <summary>
    /// 从 ERP 拉取物料
    /// </summary>
    [HttpPost("erp/sync-materials")]
    public async Task<IActionResult> SyncMaterials([FromBody] SyncRequest? request)
    {
        var result = await _erpAdapter.PullMaterialsAsync(request?.Since);
        return Ok(ApiResponse.Ok(result));
    }

    /// <summary>
    /// 从 ERP 拉取BOM
    /// </summary>
    [HttpPost("erp/sync-boms")]
    public async Task<IActionResult> SyncBoms([FromBody] SyncRequest? request)
    {
        var result = await _erpAdapter.PullBomsAsync(request?.Since);
        return Ok(ApiResponse.Ok(result));
    }

    /// <summary>
    /// 推送报工数据到 ERP
    /// </summary>
    [HttpPost("erp/push-work-report")]
    public async Task<IActionResult> PushWorkReport([FromBody] MES.Integration.Dtos.SyncWorkReportDto dto)
    {
        await _erpAdapter.PushWorkReportAsync(dto);
        return Ok(ApiResponse.Ok("推送成功"));
    }

    /// <summary>
    /// 测试 WMS 连接
    /// </summary>
    [HttpGet("wms/test")]
    public async Task<IActionResult> TestWmsConnection()
    {
        var result = await _wmsAdapter.TestConnectionAsync();
        return Ok(ApiResponse.Ok(new { connected = result, name = _wmsAdapter.AdapterName }));
    }

    /// <summary>
    /// 从 WMS 拉取库存
    /// </summary>
    [HttpPost("wms/sync-inventory")]
    public async Task<IActionResult> SyncInventory([FromBody] SyncRequest? request)
    {
        var result = await _wmsAdapter.PullInventoryAsync(request?.MaterialCode);
        return Ok(ApiResponse.Ok(result));
    }

    /// <summary>
    /// 推送入库到 WMS
    /// </summary>
    [HttpPost("wms/push-inbound")]
    public async Task<IActionResult> PushInbound([FromBody] MES.Integration.Dtos.SyncInventoryDto dto)
    {
        await _wmsAdapter.PushInboundAsync(dto);
        return Ok(ApiResponse.Ok("入库推送成功"));
    }

    /// <summary>
    /// 推送出库到 WMS
    /// </summary>
    [HttpPost("wms/push-outbound")]
    public async Task<IActionResult> PushOutbound([FromBody] MES.Integration.Dtos.SyncInventoryDto dto)
    {
        await _wmsAdapter.PushOutboundAsync(dto);
        return Ok(ApiResponse.Ok("出库推送成功"));
    }

    /// <summary>
    /// 连接 PLC
    /// </summary>
    [HttpPost("plc/connect")]
    public async Task<IActionResult> ConnectPlc()
    {
        var result = await _plcCollector.ConnectAsync();
        return Ok(ApiResponse.Ok(new { connected = result }));
    }

    /// <summary>
    /// 读取 PLC 数据
    /// </summary>
    [HttpGet("plc/read")]
    public async Task<IActionResult> ReadPlcData()
    {
        var data = await _plcCollector.ReadDataAsync();
        return Ok(ApiResponse.Ok(data));
    }

    /// <summary>
    /// 写入 PLC 数据
    /// </summary>
    [HttpPost("plc/write")]
    public async Task<IActionResult> WritePlcData([FromBody] MES.Integration.Plc.PlcData data)
    {
        await _plcCollector.WriteDataAsync(data);
        return Ok(ApiResponse.Ok("写入成功"));
    }

    /// <summary>
    /// 断开 PLC
    /// </summary>
    [HttpPost("plc/disconnect")]
    public async Task<IActionResult> DisconnectPlc()
    {
        await _plcCollector.DisconnectAsync();
        return Ok(ApiResponse.Ok("已断开"));
    }

    /// <summary>
    /// 清空事件日志
    /// </summary>
    [HttpDelete("event-logs")]
    public IActionResult ClearEventLogs() => ClearEvents();
}

public class SyncAdapterRequest
{
    public string? Direction { get; set; }
    public DateTime? Since { get; set; }
}

public class SyncRequest
{
    public DateTime? Since { get; set; }
    public string? MaterialCode { get; set; }
}
