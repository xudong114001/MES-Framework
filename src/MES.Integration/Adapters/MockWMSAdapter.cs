using Microsoft.Extensions.Logging;
using MES.Integration.Dtos;
using MES.Integration.Models;

namespace MES.Integration.Adapters;

public class MockWMSAdapter : IWMSAdapter
{
    private readonly ILogger<MockWMSAdapter> _logger;

    public string AdapterName => "MockWMS";
    public string AdapterVersion => "1.0.0";
    public bool IsConnected => true;

    public MockWMSAdapter(ILogger<MockWMSAdapter> logger)
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
            Type = "WMS",
            IsConnected = IsConnected,
            LastSyncTime = DateTime.UtcNow,
            Status = "Ready"
        };
    }

    public Task<List<SyncInventoryDto>> PullInventoryAsync(string? materialCode = null, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("MockWMS: PullInventory called for {MaterialCode}", materialCode ?? "ALL");
        return Task.FromResult(new List<SyncInventoryDto>
        {
            new()
            {
                MaterialCode = materialCode ?? "M-001",
                MaterialName = "成品A",
                WarehouseCode = "WH01",
                LocationCode = "A01-01",
                Qty = 500,
                MoveType = "STOCK"
            },
            new()
            {
                MaterialCode = materialCode ?? "M-002",
                MaterialName = "成品B",
                WarehouseCode = "WH01",
                LocationCode = "A01-02",
                Qty = 300,
                MoveType = "STOCK"
            }
        });
    }

    public Task PushInboundAsync(SyncInventoryDto dto, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("MockWMS: PushInbound for {MaterialCode} Qty={Qty}", dto.MaterialCode, dto.Qty);
        return Task.CompletedTask;
    }

    public Task PushOutboundAsync(SyncInventoryDto dto, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("MockWMS: PushOutbound for {MaterialCode} Qty={Qty}", dto.MaterialCode, dto.Qty);
        return Task.CompletedTask;
    }
}
