using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using MES.Integration.Dtos;
using MES.Integration.Models;
using MES.Domain.Enums;

namespace MES.Integration.Adapters;

public class KingdeeAdapter : IERPAdapter, IDisposable
{
    private readonly ILogger<KingdeeAdapter> _logger;
    private readonly HttpClient _httpClient;
    private readonly string _dbName;
    private readonly string _appId;
    private readonly string _appSecret;
    private string? _accessToken;
    private DateTime _tokenExpiry = DateTime.MinValue;
    private bool _disposed;

    public string AdapterName => "Kingdee";
    public string AdapterVersion => "8.0";

    public bool IsConnected => _accessToken != null && DateTime.UtcNow < _tokenExpiry;

    public KingdeeAdapter(
        HttpClient httpClient,
        string serverUrl,
        string dbName,
        string appId,
        string appSecret,
        ILogger<KingdeeAdapter> logger)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(serverUrl.TrimEnd('/'));
        _dbName = dbName;
        _appId = appId;
        _appSecret = appSecret;
        _logger = logger;
    }

    public async Task<bool> TestConnectionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await EnsureTokenAsync(cancellationToken);
            return IsConnected;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Kingdee connection test failed");
            return false;
        }
    }

    public AdapterStatus GetStatus()
    {
        return new AdapterStatus
        {
            Name = AdapterName,
            Type = "ERP",
            IsConnected = IsConnected,
            LastSyncTime = DateTime.UtcNow,
            Status = IsConnected ? "Connected" : "Disconnected",
            Error = IsConnected ? null : "未登录或令牌已过期"
        };
    }

    public async Task<List<SyncWorkOrderDto>> PullWorkOrdersAsync(DateTime? since = null, CancellationToken cancellationToken = default)
    {
        await EnsureTokenAsync(cancellationToken);

        var payload = new
        {
            FormId = "PRD_MO",
            FieldKeys = "FBillNo,FMaterialId.FNumber,FQty,FPlanStartDate,FPlanFinishDate,FBillStatus,FPriority,FDescription",
            FilterString = since.HasValue ? $"FCreateDate >= '{since.Value:yyyy-MM-dd}'" : "",
            Limit = 1000,
            StartRow = 0
        };

        var response = await _httpClient.PostAsJsonAsync("/k3cloud/Kingdee.BOS.WebApi.ServicesStub.DynamicFormService.ExecuteBillQuery.common.kdsvc", payload, cancellationToken);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken);
        var result = new List<SyncWorkOrderDto>();

        if (json.ValueKind == JsonValueKind.Array)
        {
            foreach (var row in json.EnumerateArray())
            {
                if (row.ValueKind != JsonValueKind.Array) continue;
                var fields = row.EnumerateArray().ToList();
                result.Add(new SyncWorkOrderDto
                {
                    OrderNo = fields.Count > 0 ? fields[0].ToString() : "",
                    MaterialCode = fields.Count > 1 ? fields[1].ToString() : "",
                    PlannedQty = fields.Count > 2 && decimal.TryParse(fields[2].ToString(), out var qty) ? qty : 0,
                    PlanStartTime = fields.Count > 3 && DateTime.TryParse(fields[3].ToString(), out var ps) ? ps : null,
                    PlanEndTime = fields.Count > 4 && DateTime.TryParse(fields[4].ToString(), out var pe) ? pe : null,
                    Status = MapKingdeeStatus(fields.Count > 5 ? fields[5].ToString() : null),
                    Priority = fields.Count > 6 && int.TryParse(fields[6].ToString(), out var pri) ? MapIntToPriority(pri) : Priority.NORMAL,
                    Remark = fields.Count > 7 ? fields[7].ToString() : null
                });
            }
        }

        _logger.LogInformation("Kingdee: Pulled {Count} work orders", result.Count);
        return result;
    }

    public async Task<SyncWorkOrderDto?> GetWorkOrderAsync(string orderNo, CancellationToken cancellationToken = default)
    {
        await EnsureTokenAsync(cancellationToken);

        var payload = new
        {
            FormId = "PRD_MO",
            FieldKeys = "FBillNo,FMaterialId.FNumber,FQty,FPlanStartDate,FPlanFinishDate,FBillStatus,FPriority",
            FilterString = $"FBillNo = '{orderNo}'",
            Limit = 1,
            StartRow = 0
        };

        var response = await _httpClient.PostAsJsonAsync("/k3cloud/Kingdee.BOS.WebApi.ServicesStub.DynamicFormService.ExecuteBillQuery.common.kdsvc", payload, cancellationToken);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken);

        if (json.ValueKind == JsonValueKind.Array)
        {
            foreach (var row in json.EnumerateArray())
            {
                if (row.ValueKind != JsonValueKind.Array) continue;
                var fields = row.EnumerateArray().ToList();
                return new SyncWorkOrderDto
                {
                    OrderNo = fields.Count > 0 ? fields[0].ToString() : orderNo,
                    MaterialCode = fields.Count > 1 ? fields[1].ToString() : "",
                    PlannedQty = fields.Count > 2 && decimal.TryParse(fields[2].ToString(), out var qty) ? qty : 0,
                    Status = MapKingdeeStatus(fields.Count > 5 ? fields[5].ToString() : null),
                    Priority = fields.Count > 6 && int.TryParse(fields[6].ToString(), out var pri) ? MapIntToPriority(pri) : Priority.NORMAL
                };
            }
        }

        _logger.LogWarning("Kingdee: Work order {OrderNo} not found", orderNo);
        return null;
    }

    public async Task PushWorkReportAsync(SyncWorkReportDto report, CancellationToken cancellationToken = default)
    {
        await EnsureTokenAsync(cancellationToken);

        var payload = new Dictionary<string, object>
        {
            ["FormId"] = "PRD_MORPT",
            ["data"] = new
            {
                Model = new
                {
                    FBillNo = report.OrderNo,
                    FFinishQty = report.GoodQty,
                    FScrapQty = report.ScrapQty,
                    FDescription = report.Remark ?? $"MES Report - {report.ReportTime:yyyy-MM-dd HH:mm}"
                }
            }
        };

        var response = await _httpClient.PostAsJsonAsync(
            "/k3cloud/Kingdee.BOS.WebApi.ServicesStub.DynamicFormService.Save.common.kdsvc",
            payload, cancellationToken);
        response.EnsureSuccessStatusCode();

        _logger.LogInformation("Kingdee: Pushed work report for {OrderNo}", report.OrderNo);
    }

    public async Task<List<SyncMaterialDto>> PullMaterialsAsync(DateTime? since = null, CancellationToken cancellationToken = default)
    {
        await EnsureTokenAsync(cancellationToken);

        var payload = new
        {
            FormId = "BD_MATERIAL",
            FieldKeys = "FNumber,FName,FSpecification,FBaseUnitId.FName,FMaterialGroup.FNumber,FIsEnable",
            FilterString = since.HasValue ? $"FModifyDate >= '{since.Value:yyyy-MM-dd}'" : "",
            Limit = 1000,
            StartRow = 0
        };

        var response = await _httpClient.PostAsJsonAsync("/k3cloud/Kingdee.BOS.WebApi.ServicesStub.DynamicFormService.ExecuteBillQuery.common.kdsvc", payload, cancellationToken);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken);
        var result = new List<SyncMaterialDto>();

        if (json.ValueKind == JsonValueKind.Array)
        {
            foreach (var row in json.EnumerateArray())
            {
                if (row.ValueKind != JsonValueKind.Array) continue;
                var fields = row.EnumerateArray().ToList();
                result.Add(new SyncMaterialDto
                {
                    Code = fields.Count > 0 ? fields[0].ToString() : "",
                    Name = fields.Count > 1 ? fields[1].ToString() : "",
                    Spec = fields.Count > 2 ? fields[2].ToString() : null,
                    Unit = fields.Count > 3 ? fields[3].ToString() : null,
                    Category = fields.Count > 4 ? fields[4].ToString() : null,
                    Status = fields.Count > 5 ? fields[5].ToString() != "0" : true
                });
            }
        }

        _logger.LogInformation("Kingdee: Pulled {Count} materials", result.Count);
        return result;
    }

    public async Task<List<SyncBomDto>> PullBomsAsync(DateTime? since = null, CancellationToken cancellationToken = default)
    {
        await EnsureTokenAsync(cancellationToken);

        var payload = new
        {
            FormId = "ENG_BOM",
            FieldKeys = "FNumber,FMaterialId.FNumber,FBomVersion",
            FilterString = since.HasValue ? $"FModifyDate >= '{since.Value:yyyy-MM-dd}'" : "",
            Limit = 1000,
            StartRow = 0
        };

        var response = await _httpClient.PostAsJsonAsync("/k3cloud/Kingdee.BOS.WebApi.ServicesStub.DynamicFormService.ExecuteBillQuery.common.kdsvc", payload, cancellationToken);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken);
        var result = new List<SyncBomDto>();

        if (json.ValueKind == JsonValueKind.Array)
        {
            foreach (var row in json.EnumerateArray())
            {
                if (row.ValueKind != JsonValueKind.Array) continue;
                var fields = row.EnumerateArray().ToList();

                var bomNo = fields.Count > 0 ? fields[0].ToString() : "";
                var productCode = fields.Count > 1 ? fields[1].ToString() : "";
                var version = fields.Count > 2 ? fields[2].ToString() : "1.0";

                var detailPayload = new
                {
                    FormId = "ENG_BOM",
                    FieldKeys = "FMaterialId.FNumber,FMustQty,FUnitId.FName,FIsKeyItem",
                    FilterString = $"FNumber = '{bomNo}'",
                    Limit = 100,
                    StartRow = 0
                };

                var detailResponse = await _httpClient.PostAsJsonAsync(
                    "/k3cloud/Kingdee.BOS.WebApi.ServicesStub.DynamicFormService.ExecuteBillQuery.common.kdsvc",
                    detailPayload, cancellationToken);

                var bom = new SyncBomDto
                {
                    ProductCode = productCode,
                    Version = version
                };

                if (detailResponse.IsSuccessStatusCode)
                {
                    var detailJson = await detailResponse.Content.ReadFromJsonAsync<JsonElement>(cancellationToken);
                    if (detailJson.ValueKind == JsonValueKind.Array)
                    {
                        var seq = 1;
                        foreach (var detailRow in detailJson.EnumerateArray())
                        {
                            if (detailRow.ValueKind != JsonValueKind.Array) continue;
                            var dFields = detailRow.EnumerateArray().ToList();
                            bom.Items.Add(new SyncBomItemDto
                            {
                                MaterialCode = dFields.Count > 0 ? dFields[0].ToString() : "",
                                Qty = dFields.Count > 1 && decimal.TryParse(dFields[1].ToString(), out var q) ? q : 0,
                                Unit = dFields.Count > 2 ? dFields[2].ToString() : null,
                                Sequence = seq++,
                                IsKeyPart = dFields.Count > 3 && dFields[3].ToString() == "1"
                            });
                        }
                    }
                }

                result.Add(bom);
            }
        }

        _logger.LogInformation("Kingdee: Pulled {Count} BOMs", result.Count);
        return result;
    }

    private async Task EnsureTokenAsync(CancellationToken cancellationToken)
    {
        if (IsConnected) return;

        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
        var sign = ComputeSign(timestamp);

        var payload = new
        {
            dbid = _dbName,
            appid = _appId,
            sign = sign,
            timestamp = timestamp
        };

        var response = await _httpClient.PostAsJsonAsync(
            "/k3cloud/Kingdee.BOS.WebApi.ServicesStub.ValidateService.CommonValidate.common.kdsvc",
            payload, cancellationToken);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken);
        _accessToken = json.TryGetProperty("SessionId", out var sid) ? sid.GetString() : null;
        _tokenExpiry = DateTime.UtcNow.AddMinutes(25);

        if (_accessToken != null)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
        }

        _logger.LogInformation("Kingdee: Token acquired");
    }

    private string ComputeSign(string timestamp)
    {
        var raw = $"{_appId}{timestamp}{_appSecret}";
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(raw));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private static WorkOrderStatus MapKingdeeStatus(string? status)
    {
        return status switch
        {
            "A" or "Create" => WorkOrderStatus.PENDING,
            "B" or "Submit" => WorkOrderStatus.RELEASED,
            "C" or "Audit" => WorkOrderStatus.SCHEDULED,
            "D" or "Start" => WorkOrderStatus.IN_PROGRESS,
            "E" or "Finish" => WorkOrderStatus.COMPLETED,
            "F" or "Close" => WorkOrderStatus.CLOSED,
            _ => WorkOrderStatus.PENDING
        };
    }

    private static Priority MapIntToPriority(int value)
    {
        return value switch
        {
            <= 20 => Priority.LOW,
            <= 60 => Priority.NORMAL,
            <= 90 => Priority.HIGH,
            _ => Priority.URGENT
        };
    }

    public void Dispose()
    {
        if (_disposed) return;
        _httpClient.Dispose();
        _disposed = true;
    }
}
