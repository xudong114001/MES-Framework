using Microsoft.Extensions.Logging;
using MES.Integration.Dtos;
using MES.Integration.Models;

namespace MES.Integration.Adapters;

public class MockERPAdapter : IERPAdapter
{
    private readonly ILogger<MockERPAdapter> _logger;

    public string AdapterName => "MockERP";
    public string AdapterVersion => "1.0.0";
    public bool IsConnected => true;

    public MockERPAdapter(ILogger<MockERPAdapter> logger)
    {
        _logger = logger;
    }

    public Task<bool> TestConnectionAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(true);
    }

    public AdapterStatus GetStatus()
    {
        return new AdapterStatus
        {
            Name = AdapterName,
            Type = "ERP",
            IsConnected = IsConnected,
            LastSyncTime = DateTime.UtcNow,
            Status = "Ready"
        };
    }

    public Task<List<SyncWorkOrderDto>> PullWorkOrdersAsync(DateTime? since = null, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("MockERP: PullWorkOrders called");
        return Task.FromResult(new List<SyncWorkOrderDto>
        {
            new()
            {
                OrderNo = "MO-2026-001",
                MaterialCode = "M-001",
                PlannedQty = 100,
                Status = MES.Domain.Enums.WorkOrderStatus.PENDING,
                Priority = 1
            },
            new()
            {
                OrderNo = "MO-2026-002",
                MaterialCode = "M-002",
                PlannedQty = 50,
                Status = MES.Domain.Enums.WorkOrderStatus.PENDING,
                Priority = 2
            }
        });
    }

    public Task<SyncWorkOrderDto?> GetWorkOrderAsync(string orderNo, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("MockERP: GetWorkOrder called for {OrderNo}", orderNo);
        return Task.FromResult<SyncWorkOrderDto?>(new SyncWorkOrderDto
        {
            OrderNo = orderNo,
            MaterialCode = "M-001",
            PlannedQty = 100,
            Status = MES.Domain.Enums.WorkOrderStatus.PENDING,
            Priority = 1
        });
    }

    public Task PushWorkReportAsync(SyncWorkReportDto report, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("MockERP: PushWorkReport called for {OrderNo}", report.OrderNo);
        return Task.CompletedTask;
    }

    public Task<List<SyncMaterialDto>> PullMaterialsAsync(DateTime? since = null, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("MockERP: PullMaterials called");
        return Task.FromResult(new List<SyncMaterialDto>
        {
            new() { Code = "M-001", Name = "成品A", Spec = "Spec-A", Unit = "PCS", Category = "Product" },
            new() { Code = "M-002", Name = "成品B", Spec = "Spec-B", Unit = "PCS", Category = "Product" }
        });
    }

    public Task<List<SyncBomDto>> PullBomsAsync(DateTime? since = null, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("MockERP: PullBoms called");
        return Task.FromResult(new List<SyncBomDto>
        {
            new()
            {
                ProductCode = "M-001",
                Version = "1.0",
                Items = new List<SyncBomItemDto>
                {
                    new() { MaterialCode = "C-001", Qty = 2, Unit = "PCS" },
                    new() { MaterialCode = "C-002", Qty = 1, Unit = "PCS" }
                }
            }
        });
    }
}
