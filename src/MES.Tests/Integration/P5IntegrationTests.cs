using Microsoft.Extensions.Logging;
using MES.Domain.Enums;
using MES.Integration.Adapters;
using MES.Integration.Dtos;
using MES.Integration.EventBus;
using MES.Integration.Models;
using MES.Integration.Plc;
using Moq;
using Xunit;
using Xunit.Abstractions;

namespace MES.Tests.Integration;

public class P5IntegrationTests : IDisposable
{
    private readonly Mock<ILogger<MockERPAdapter>> _erpLogger = new();
    private readonly Mock<ILogger<MockWMSAdapter>> _wmsLogger = new();
    private readonly Mock<ILogger<MockPlcCollector>> _plcLogger = new();
    private readonly Mock<ILogger<RabbitMQEventBus>> _ebLogger = new();

    public P5IntegrationTests(ITestOutputHelper output)
    {
    }

    #region ERP 适配器测试

    [Fact]
    public void MockERPAdapter_GetStatus_ReturnsConnected()
    {
        var adapter = new MockERPAdapter(_erpLogger.Object);
        var status = adapter.GetStatus();

        Assert.Equal("MockERP", status.Name);
        Assert.Equal("ERP", status.Type);
        Assert.True(status.IsConnected);
        Assert.Equal("Ready", status.Status);
    }

    [Fact]
    public async Task MockERPAdapter_PullWorkOrdersAsync_ReturnsData()
    {
        var adapter = new MockERPAdapter(_erpLogger.Object);
        var result = await adapter.PullWorkOrdersAsync();

        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal("MO-2026-001", result[0].OrderNo);
    }

    [Fact]
    public async Task MockERPAdapter_PullMaterialsAsync_ReturnsData()
    {
        var adapter = new MockERPAdapter(_erpLogger.Object);
        var result = await adapter.PullMaterialsAsync();

        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal("M-001", result[0].Code);
    }

    [Fact]
    public async Task MockERPAdapter_PullBomsAsync_ReturnsData()
    {
        var adapter = new MockERPAdapter(_erpLogger.Object);
        var result = await adapter.PullBomsAsync();

        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("M-001", result[0].ProductCode);
        Assert.Equal(2, result[0].Items.Count);
    }

    [Fact]
    public async Task MockERPAdapter_PushWorkReportAsync_Completes()
    {
        var adapter = new MockERPAdapter(_erpLogger.Object);
        var report = new SyncWorkReportDto
        {
            OrderNo = "MO-2026-001",
            GoodQty = 100,
            ScrapQty = 2,
            ReworkQty = 1
        };

        await adapter.PushWorkReportAsync(report);
        Assert.True(adapter.IsConnected);
    }

    [Fact]
    public async Task MockERPAdapter_TestConnectionAsync_ReturnsTrue()
    {
        var adapter = new MockERPAdapter(_erpLogger.Object);
        var result = await adapter.TestConnectionAsync();
        Assert.True(result);
    }

    #endregion

    #region WMS 适配器测试

    [Fact]
    public void MockWMSAdapter_GetStatus_ReturnsConnected()
    {
        var adapter = new MockWMSAdapter(_wmsLogger.Object);
        var status = adapter.GetStatus();

        Assert.Equal("MockWMS", status.Name);
        Assert.Equal("WMS", status.Type);
        Assert.True(status.IsConnected);
    }

    [Fact]
    public async Task MockWMSAdapter_PullInventoryAsync_ReturnsData()
    {
        var adapter = new MockWMSAdapter(_wmsLogger.Object);
        var result = await adapter.PullInventoryAsync();

        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal("WH01", result[0].WarehouseCode);
    }

    [Fact]
    public async Task MockWMSAdapter_PushInboundAsync_Completes()
    {
        var adapter = new MockWMSAdapter(_wmsLogger.Object);
        var dto = new SyncInventoryDto
        {
            MaterialCode = "M-001",
            MaterialName = "成品A",
            Qty = 100,
            MoveType = "INBOUND"
        };

        await adapter.PushInboundAsync(dto);
        Assert.True(adapter.IsConnected);
    }

    [Fact]
    public async Task MockWMSAdapter_PushOutboundAsync_Completes()
    {
        var adapter = new MockWMSAdapter(_wmsLogger.Object);
        var dto = new SyncInventoryDto
        {
            MaterialCode = "M-001",
            Qty = 50,
            MoveType = "OUTBOUND"
        };

        await adapter.PushOutboundAsync(dto);
        Assert.True(adapter.IsConnected);
    }

    #endregion

    #region PLC 采集器测试

    [Fact]
    public void MockPlcCollector_DeviceName_ReturnsDefault()
    {
        var collector = new MockPlcCollector(_plcLogger.Object);
        Assert.Equal("MockPLC", collector.DeviceName);
    }

    [Fact]
    public void MockPlcCollector_IsConnected_DefaultFalse()
    {
        var collector = new MockPlcCollector(_plcLogger.Object);
        Assert.False(collector.IsConnected);
    }

    [Fact]
    public async Task MockPlcCollector_ConnectAsync_ReturnsTrue()
    {
        var collector = new MockPlcCollector(_plcLogger.Object);
        var result = await collector.ConnectAsync();

        Assert.True(result);
        Assert.True(collector.IsConnected);
    }

    [Fact]
    public async Task MockPlcCollector_DisconnectAsync_SetsDisconnected()
    {
        var collector = new MockPlcCollector(_plcLogger.Object);
        await collector.ConnectAsync();
        await collector.DisconnectAsync();

        Assert.False(collector.IsConnected);
    }

    [Fact]
    public async Task MockPlcCollector_ReadDataAsync_ReturnsData()
    {
        var collector = new MockPlcCollector(_plcLogger.Object);
        await collector.ConnectAsync();
        var data = await collector.ReadDataAsync();

        Assert.NotNull(data);
        Assert.Equal("MockPLC", data.DeviceName);
        Assert.Equal(DeviceStatus.Running, data.Status);
    }

    [Fact]
    public async Task MockPlcCollector_WriteDataAsync_Completes()
    {
        var collector = new MockPlcCollector(_plcLogger.Object);
        await collector.ConnectAsync();

        var data = new PlcData
        {
            DeviceName = "MockPLC",
            Registers = new Dictionary<string, object> { { "HR_0", (ushort)100 } }
        };

        await collector.WriteDataAsync(data);
        Assert.True(collector.IsConnected);
    }

    #endregion

    #region 事件总线测试

    [Fact]
    public async Task EventBus_PublishAsync_DoesNotThrow()
    {
        var eventBus = new RabbitMQEventBus(_ebLogger.Object);

        var testEvent = new TestIntegrationEvent
        {
            WorkOrderId = 123,
            OrderNo = "WO-TEST-001"
        };

        await eventBus.PublishAsync(testEvent);
        Assert.True(true);
    }

    [Fact]
    public async Task EventBus_SubscribeAsync_DoesNotThrow()
    {
        var eventBus = new RabbitMQEventBus(_ebLogger.Object);

        await eventBus.SubscribeAsync<TestIntegrationEvent>(async e =>
        {
            await Task.CompletedTask;
        });

        Assert.True(true);
    }

    private class TestIntegrationEvent : EventBase
    {
        public override string EventType => "Test.Event";
        public long WorkOrderId { get; set; }
        public string OrderNo { get; set; } = string.Empty;
    }

    #endregion

    #region DTO 测试

    [Fact]
    public void SyncWorkOrderDto_Properties_SetCorrectly()
    {
        var dto = new SyncWorkOrderDto
        {
            OrderNo = "WO-001",
            MaterialCode = "M-001",
            PlannedQty = 100,
            Priority = Priority.Normal
        };

        Assert.Equal("WO-001", dto.OrderNo);
        Assert.Equal("M-001", dto.MaterialCode);
        Assert.Equal(100, dto.PlannedQty);
    }

    [Fact]
    public void SyncInventoryDto_Properties_SetCorrectly()
    {
        var dto = new SyncInventoryDto
        {
            MaterialCode = "M-001",
            MaterialName = "成品A",
            Qty = 500,
            WarehouseCode = "WH01",
            MoveType = "STOCK"
        };

        Assert.Equal("M-001", dto.MaterialCode);
        Assert.Equal(500, dto.Qty);
        Assert.Equal("WH01", dto.WarehouseCode);
    }

    [Fact]
    public void AdapterStatus_Properties_SetCorrectly()
    {
        var status = new AdapterStatus
        {
            Name = "TestAdapter",
            Type = "ERP",
            IsConnected = true,
            LastSyncTime = DateTime.UtcNow,
            Status = "Ready"
        };

        Assert.Equal("TestAdapter", status.Name);
        Assert.Equal("ERP", status.Type);
        Assert.True(status.IsConnected);
        Assert.Equal("Ready", status.Status);
    }

    #endregion

    public void Dispose()
    {
    }
}
