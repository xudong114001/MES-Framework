using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using MES.Integration.Dtos;
using MES.Integration.Models;
using MES.Domain.Enums;

namespace MES.Integration.Adapters;

public class SapB1Adapter : IERPAdapter, IDisposable
{
    private readonly ILogger<SapB1Adapter> _logger;
    private readonly HttpClient _httpClient;
    private readonly string _companyDb;
    private readonly string _userName;
    private readonly string _password;
    private string? _sessionId;
    private DateTime _sessionExpiry = DateTime.MinValue;
    private bool _disposed;

    public string AdapterName => "SAP B1";
    public string AdapterVersion => "10.0";

    public bool IsConnected => _sessionId != null && DateTime.UtcNow < _sessionExpiry;

    public SapB1Adapter(
        HttpClient httpClient,
        string serverUrl,
        string companyDb,
        string userName,
        string password,
        ILogger<SapB1Adapter> logger)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(serverUrl.TrimEnd('/'));
        _companyDb = companyDb;
        _userName = userName;
        _password = password;
        _logger = logger;
    }

    public async Task<bool> TestConnectionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await EnsureSessionAsync(cancellationToken);
            return IsConnected;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SAP B1 connection test failed");
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
            Error = IsConnected ? null : "未登录或会话已过期"
        };
    }

    public async Task<List<SyncWorkOrderDto>> PullWorkOrdersAsync(DateTime? since = null, CancellationToken cancellationToken = default)
    {
        await EnsureSessionAsync(cancellationToken);

        var filter = since.HasValue
            ? $"?&$filter=CreationDate ge datetime'{since.Value:yyyy-MM-ddTHH:mm:ss}'"
            : "";

        var response = await _httpClient.GetAsync($"/b1s/v1/ProductionOrders{filter}", cancellationToken);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken);
        var result = new List<SyncWorkOrderDto>();

        if (json.TryGetProperty("value", out var value))
        {
            foreach (var item in value.EnumerateArray())
            {
                result.Add(new SyncWorkOrderDto
                {
                    OrderNo = item.TryGetProperty("DocumentNumber", out var docNum) ? docNum.ToString() : "",
                    SourceRef = item.TryGetProperty("AbsoluteEntry", out var absEntry) ? absEntry.ToString() : null,
                    MaterialCode = item.TryGetProperty("ItemNo", out var itemNo) ? itemNo.ToString() : "",
                    PlannedQty = item.TryGetProperty("PlannedQuantity", out var qty) ? qty.GetDecimal() : 0,
                    PlanStartTime = item.TryGetProperty("StartDate", out var start) && DateTime.TryParse(start.ToString(), out var s) ? s : null,
                    PlanEndTime = item.TryGetProperty("DueDate", out var end) && DateTime.TryParse(end.ToString(), out var e) ? e : null,
                    Status = MapSapStatus(item.TryGetProperty("ProductionOrderStatus", out var st) ? st.GetString() : null),
                    Priority = MapIntToPriority(item.TryGetProperty("Priority", out var pri) ? pri.GetInt32() : 0),
                    Remark = item.TryGetProperty("Remarks", out var rem) ? rem.GetString() : null
                });
            }
        }

        _logger.LogInformation("SAP B1: Pulled {Count} work orders", result.Count);
        return result;
    }

    public async Task<SyncWorkOrderDto?> GetWorkOrderAsync(string orderNo, CancellationToken cancellationToken = default)
    {
        await EnsureSessionAsync(cancellationToken);

        var response = await _httpClient.GetAsync($"/b1s/v1/ProductionOrders({orderNo})", cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("SAP B1: Work order {OrderNo} not found", orderNo);
            return null;
        }

        var item = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken);

        return new SyncWorkOrderDto
        {
            OrderNo = item.TryGetProperty("DocumentNumber", out var docNum) ? docNum.ToString() : orderNo,
            SourceRef = item.TryGetProperty("AbsoluteEntry", out var absEntry) ? absEntry.ToString() : null,
            MaterialCode = item.TryGetProperty("ItemNo", out var itemNo) ? itemNo.ToString() : "",
            PlannedQty = item.TryGetProperty("PlannedQuantity", out var qty) ? qty.GetDecimal() : 0,
            Status = MapSapStatus(item.TryGetProperty("ProductionOrderStatus", out var st) ? st.GetString() : null),
            Priority = MapIntToPriority(item.TryGetProperty("Priority", out var pri) ? pri.GetInt32() : 0)
        };
    }

    public async Task PushWorkReportAsync(SyncWorkReportDto report, CancellationToken cancellationToken = default)
    {
        await EnsureSessionAsync(cancellationToken);

        var payload = new
        {
            DocumentNumber = report.OrderNo,
            CompletedQuantity = report.GoodQty,
            ScrapQuantity = report.ScrapQty,
            Remarks = report.Remark ?? $"Reported by MES - {report.ReportTime:yyyy-MM-dd HH:mm}"
        };

        var response = await _httpClient.PatchAsync(
            $"/b1s/v1/ProductionOrders({report.OrderNo})",
            JsonContent.Create(payload),
            cancellationToken);

        response.EnsureSuccessStatusCode();
        _logger.LogInformation("SAP B1: Pushed work report for {OrderNo}", report.OrderNo);
    }

    public async Task<List<SyncMaterialDto>> PullMaterialsAsync(DateTime? since = null, CancellationToken cancellationToken = default)
    {
        await EnsureSessionAsync(cancellationToken);

        var filter = since.HasValue
            ? $"?&$filter=UpdateDate ge datetime'{since.Value:yyyy-MM-ddTHH:mm:ss}'"
            : "";

        var response = await _httpClient.GetAsync($"/b1s/v1/Items{filter}", cancellationToken);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken);
        var result = new List<SyncMaterialDto>();

        if (json.TryGetProperty("value", out var value))
        {
            foreach (var item in value.EnumerateArray())
            {
                result.Add(new SyncMaterialDto
                {
                    Code = item.TryGetProperty("ItemCode", out var code) ? code.ToString() : "",
                    Name = item.TryGetProperty("ItemName", out var name) ? name.ToString() : "",
                    Spec = item.TryGetProperty("SalesUnit", out var spec) ? spec.ToString() : null,
                    Unit = item.TryGetProperty("PurchaseUnit", out var unit) ? unit.ToString() : null,
                    Category = item.TryGetProperty("ItemsGroupCode", out var cat) ? cat.ToString() : null,
                    Status = true
                });
            }
        }

        _logger.LogInformation("SAP B1: Pulled {Count} materials", result.Count);
        return result;
    }

    public async Task<List<SyncBomDto>> PullBomsAsync(DateTime? since = null, CancellationToken cancellationToken = default)
    {
        await EnsureSessionAsync(cancellationToken);

        var response = await _httpClient.GetAsync("/b1s/v1/ProductTrees", cancellationToken);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken);
        var result = new List<SyncBomDto>();

        if (json.TryGetProperty("value", out var value))
        {
            foreach (var tree in value.EnumerateArray())
            {
                var bom = new SyncBomDto
                {
                    ProductCode = tree.TryGetProperty("TreeCode", out var code) ? code.ToString() : "",
                    Version = tree.TryGetProperty("Revision", out var rev) ? rev.ToString() : "1.0"
                };

                if (tree.TryGetProperty("ProductTreeLines", out var lines))
                {
                    var seq = 1;
                    foreach (var line in lines.EnumerateArray())
                    {
                        bom.Items.Add(new SyncBomItemDto
                        {
                            MaterialCode = line.TryGetProperty("ItemCode", out var ic) ? ic.ToString() : "",
                            Qty = line.TryGetProperty("Quantity", out var q) ? q.GetDecimal() : 0,
                            Unit = line.TryGetProperty("Warehouse", out var wh) ? wh.ToString() : null,
                            Sequence = seq++,
                            IsKeyPart = false
                        });
                    }
                }

                result.Add(bom);
            }
        }

        _logger.LogInformation("SAP B1: Pulled {Count} BOMs", result.Count);
        return result;
    }

    private async Task EnsureSessionAsync(CancellationToken cancellationToken)
    {
        if (IsConnected) return;

        var loginPayload = new
        {
            CompanyDB = _companyDb,
            UserName = _userName,
            Password = _password
        };

        var response = await _httpClient.PostAsync(
            "/b1s/v1/Login",
            JsonContent.Create(loginPayload),
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken);
        _sessionId = json.TryGetProperty("SessionId", out var sid) ? sid.ToString() : null;
        _sessionExpiry = DateTime.UtcNow.AddMinutes(25);

        if (_sessionId != null)
        {
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Cookie", $"B1SESSION={_sessionId}");
        }

        _logger.LogInformation("SAP B1: Session established");
    }

    private static WorkOrderStatus MapSapStatus(string? status)
    {
        return status switch
        {
            "bop_Pending" => WorkOrderStatus.PENDING,
            "bop_Released" => WorkOrderStatus.RELEASED,
            "bop_Start" or "bop_InProgress" => WorkOrderStatus.IN_PROGRESS,
            "bop_Completed" => WorkOrderStatus.COMPLETED,
            "bop_Closed" => WorkOrderStatus.CLOSED,
            "bop_Cancelled" => WorkOrderStatus.CANCELLED,
            "bop_Planned" => WorkOrderStatus.SCHEDULED,
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
