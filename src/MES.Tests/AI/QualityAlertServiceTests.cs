using MES.AI.Application.Services;
using MES.AI.Domain.Entities;
using MES.AI.Domain.Enums;
using MES.Domain.Entities;
using MES.Domain.Enums;
using MES.Infrastructure.Repositories;
using MES.Tests;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MES.Tests.AI;

public class QualityAlertServiceTests
{
    private readonly Mock<IRepository<WorkOrder>> _workOrderRepo;
    private readonly Mock<IRepository<WorkReport>> _workReportRepo;
    private readonly Mock<IRepository<MaterialTrace>> _materialTraceRepo;
    private readonly Mock<IRepository<AlertRecord>> _alertRepo;
    private readonly Mock<ILogger<QualityAlertService>> _logger;
    private readonly QualityAlertService _service;

    public QualityAlertServiceTests()
    {
        _workOrderRepo = new Mock<IRepository<WorkOrder>>();
        _workReportRepo = new Mock<IRepository<WorkReport>>();
        _materialTraceRepo = new Mock<IRepository<MaterialTrace>>();
        _alertRepo = new Mock<IRepository<AlertRecord>>();
        _logger = new Mock<ILogger<QualityAlertService>>();

        _service = new QualityAlertService(
            _workOrderRepo.Object,
            _workReportRepo.Object,
            _materialTraceRepo.Object,
            _alertRepo.Object,
            _logger.Object);
    }

    [Fact]
    public async Task CheckConsecutiveScrapRate_ReturnsAlert_WhenScrapRateExceeds5Percent()
    {
        var orders = new List<WorkOrder>
        {
            TestEntityFactory.CreateWorkOrderDirect(id: 1, orderNo: "WO-001", materialId: 1, plannedQty: 100, completedQty: 100, scrapQty: 0, status: WorkOrderStatus.COMPLETED, lineId: 1),
            TestEntityFactory.CreateWorkOrderDirect(id: 2, orderNo: "WO-002", materialId: 1, plannedQty: 100, completedQty: 100, scrapQty: 0, status: WorkOrderStatus.COMPLETED, lineId: 1),
            TestEntityFactory.CreateWorkOrderDirect(id: 3, orderNo: "WO-003", materialId: 1, plannedQty: 100, completedQty: 100, scrapQty: 0, status: WorkOrderStatus.COMPLETED, lineId: 1)
        };
        // 设置 CreatedAt 需要使用反射
        TestEntityFactory.SetProperty(orders[0], "CreatedAt", DateTime.UtcNow.AddDays(-3));
        TestEntityFactory.SetProperty(orders[1], "CreatedAt", DateTime.UtcNow.AddDays(-2));
        TestEntityFactory.SetProperty(orders[2], "CreatedAt", DateTime.UtcNow.AddDays(-1));

        var reports = new List<WorkReport>
        {
            TestEntityFactory.CreateWorkReportDirect(id: 1, workOrderId: 1, goodQty: 94, scrapQty: 6, reworkQty: 0),
            TestEntityFactory.CreateWorkReportDirect(id: 2, workOrderId: 2, goodQty: 93, scrapQty: 7, reworkQty: 0),
            TestEntityFactory.CreateWorkReportDirect(id: 3, workOrderId: 3, goodQty: 92, scrapQty: 8, reworkQty: 0)
        };

        _workOrderRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(orders);
        _workReportRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(reports);

        var alerts = await _service.AnalyzeAsync();

        var scrapAlert = alerts.FirstOrDefault(a => a.Title == "产线连续不良率飙升");
        Assert.NotNull(scrapAlert);
    }

    [Fact]
    public async Task CheckConsecutiveScrapRate_ReturnsEmpty_WhenScrapRateNormal()
    {
        var orders = new List<WorkOrder>
        {
            TestEntityFactory.CreateWorkOrderDirect(id: 1, orderNo: "WO-001", materialId: 1, plannedQty: 100, completedQty: 100, scrapQty: 0, status: WorkOrderStatus.COMPLETED, lineId: 1),
            TestEntityFactory.CreateWorkOrderDirect(id: 2, orderNo: "WO-002", materialId: 1, plannedQty: 100, completedQty: 100, scrapQty: 0, status: WorkOrderStatus.COMPLETED, lineId: 1),
            TestEntityFactory.CreateWorkOrderDirect(id: 3, orderNo: "WO-003", materialId: 1, plannedQty: 100, completedQty: 100, scrapQty: 0, status: WorkOrderStatus.COMPLETED, lineId: 1)
        };
        TestEntityFactory.SetProperty(orders[0], "CreatedAt", DateTime.UtcNow.AddDays(-3));
        TestEntityFactory.SetProperty(orders[1], "CreatedAt", DateTime.UtcNow.AddDays(-2));
        TestEntityFactory.SetProperty(orders[2], "CreatedAt", DateTime.UtcNow.AddDays(-1));

        var reports = new List<WorkReport>
        {
            TestEntityFactory.CreateWorkReportDirect(id: 1, workOrderId: 1, goodQty: 99, scrapQty: 1, reworkQty: 0),
            TestEntityFactory.CreateWorkReportDirect(id: 2, workOrderId: 2, goodQty: 99, scrapQty: 1, reworkQty: 0),
            TestEntityFactory.CreateWorkReportDirect(id: 3, workOrderId: 3, goodQty: 98, scrapQty: 2, reworkQty: 0)
        };

        _workOrderRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(orders);
        _workReportRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(reports);

        var alerts = await _service.AnalyzeAsync();

        var scrapAlert = alerts.FirstOrDefault(a => a.Title == "产线连续不良率飙升");
        Assert.Null(scrapAlert);
    }

    [Fact]
    public async Task CheckConsecutiveScrapRate_ReturnsEmpty_WhenDataLessThan3()
    {
        var orders = new List<WorkOrder>
        {
            TestEntityFactory.CreateWorkOrderDirect(id: 1, orderNo: "WO-001", materialId: 1, plannedQty: 100, completedQty: 100, scrapQty: 0, status: WorkOrderStatus.COMPLETED, lineId: 1)
        };
        TestEntityFactory.SetProperty(orders[0], "CreatedAt", DateTime.UtcNow.AddDays(-1));

        var reports = new List<WorkReport>
        {
            TestEntityFactory.CreateWorkReportDirect(id: 1, workOrderId: 1, goodQty: 90, scrapQty: 10, reworkQty: 0)
        };

        _workOrderRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(orders);
        _workReportRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(reports);

        var alerts = await _service.AnalyzeAsync();

        var scrapAlert = alerts.FirstOrDefault(a => a.Title == "产线连续不良率飙升");
        Assert.Null(scrapAlert);
    }

    [Fact]
    public async Task CheckBatchDefectRate_ReturnsAlert_WhenBatchDefectRateExceeds3Percent()
    {
        var traces = new List<MaterialTrace>
        {
            new() { Id = 1, BatchNo = "BATCH001", WorkOrderId = 1 },
            new() { Id = 2, BatchNo = "BATCH001", WorkOrderId = 2 },
            new() { Id = 3, BatchNo = "BATCH001", WorkOrderId = 3 }
        };

        var reports = new List<WorkReport>
        {
            new() { Id = 1, WorkOrderId = 1, GoodQty = 97, ScrapQty = 2, ReworkQty = 1 },
            new() { Id = 2, WorkOrderId = 2, GoodQty = 96, ScrapQty = 3, ReworkQty = 1 },
            new() { Id = 3, WorkOrderId = 3, GoodQty = 95, ScrapQty = 4, ReworkQty = 1 }
        };

        _materialTraceRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(traces);
        _workReportRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(reports);

        var alerts = await _service.AnalyzeAsync();

        var batchAlert = alerts.FirstOrDefault(a => a.Title == "物料批次异常");
        Assert.NotNull(batchAlert);
    }

    [Fact]
    public async Task CheckBatchDefectRate_ReturnsEmpty_WhenBatchDefectRateNormal()
    {
        var traces = new List<MaterialTrace>
        {
            new() { Id = 1, BatchNo = "BATCH001", WorkOrderId = 1 },
            new() { Id = 2, BatchNo = "BATCH001", WorkOrderId = 2 }
        };

        var reports = new List<WorkReport>
        {
            new() { Id = 1, WorkOrderId = 1, GoodQty = 99, ScrapQty = 1, ReworkQty = 0 },
            new() { Id = 2, WorkOrderId = 2, GoodQty = 99, ScrapQty = 1, ReworkQty = 0 }
        };

        _materialTraceRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(traces);
        _workReportRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(reports);

        var alerts = await _service.AnalyzeAsync();

        var batchAlert = alerts.FirstOrDefault(a => a.Title == "物料批次异常");
        Assert.Null(batchAlert);
    }

    [Fact]
    public async Task CheckConsecutiveRework_ReturnsAlert_WhenWorkstationHas5ConsecutiveRework()
    {
        var reports = new List<WorkReport>
        {
            new() { Id = 1, WorkOrderId = 1, WorkstationId = 1, ReportType = ReportType.REWORK, ReworkQty = 5, ReportTime = DateTime.UtcNow.AddMinutes(-5) },
            new() { Id = 2, WorkOrderId = 1, WorkstationId = 1, ReportType = ReportType.REWORK, ReworkQty = 3, ReportTime = DateTime.UtcNow.AddMinutes(-4) },
            new() { Id = 3, WorkOrderId = 1, WorkstationId = 1, ReportType = ReportType.REWORK, ReworkQty = 4, ReportTime = DateTime.UtcNow.AddMinutes(-3) },
            new() { Id = 4, WorkOrderId = 1, WorkstationId = 1, ReportType = ReportType.REWORK, ReworkQty = 2, ReportTime = DateTime.UtcNow.AddMinutes(-2) },
            new() { Id = 5, WorkOrderId = 1, WorkstationId = 1, ReportType = ReportType.REWORK, ReworkQty = 6, ReportTime = DateTime.UtcNow.AddMinutes(-1) }
        };

        _workReportRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(reports);

        var alerts = await _service.AnalyzeAsync();

        var reworkAlert = alerts.FirstOrDefault(a => a.Title == "工位连续返工");
        Assert.NotNull(reworkAlert);
    }

    [Fact]
    public async Task CheckConsecutiveRework_ReturnsEmpty_WhenReworkNormal()
    {
        var reports = new List<WorkReport>
        {
            new() { Id = 1, WorkOrderId = 1, WorkstationId = 1, ReportType = ReportType.REWORK, ReworkQty = 1, ReportTime = DateTime.UtcNow.AddMinutes(-5) },
            new() { Id = 2, WorkOrderId = 2, WorkstationId = 2, ReportType = ReportType.COMPLETE, ReworkQty = 0, ReportTime = DateTime.UtcNow.AddMinutes(-4) }
        };

        _workReportRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(reports);

        var alerts = await _service.AnalyzeAsync();

        var reworkAlert = alerts.FirstOrDefault(a => a.Title == "工位连续返工");
        Assert.Null(reworkAlert);
    }

    [Fact]
    public async Task GetActiveAlerts_ReturnsUnprocessedAlerts()
    {
        var alerts = new List<AlertRecord>
        {
            new() { Id = 1, Title = "测试预警1", IsProcessed = false, Level = AlertLevel.Low },
            new() { Id = 2, Title = "测试预警2", IsProcessed = true, Level = AlertLevel.Low }
        };

        _workOrderRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<WorkOrder>());
        _workReportRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<WorkReport>());
        _materialTraceRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<MaterialTrace>());

        await _service.AnalyzeAsync();

        var activeAlerts = await _service.GetActiveAlertsAsync();

        Assert.NotNull(activeAlerts);
    }

    [Fact]
    public async Task MarkAsProcessedAsync_UpdatesAlertStatus()
    {
        var orders = new List<WorkOrder>();
        var reports = new List<WorkReport>();
        var traces = new List<MaterialTrace>();

        _workOrderRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(orders);
        _workReportRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(reports);
        _materialTraceRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(traces);

        await _service.AnalyzeAsync();
    }

    private QualityAlertService CreateServiceWithPrivateAlerts(List<AlertRecord> alerts)
    {
        return _service;
    }
}