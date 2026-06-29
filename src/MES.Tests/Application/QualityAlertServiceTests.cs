using MES.Application.Dtos;
using MES.Application.Services;
using MES.Domain.Entities;
using MES.Domain.Enums;
using MES.Domain.Repositories;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MES.Tests.Application;

public class QualityAlertServiceTests
{
    private readonly Mock<IRepository<WorkOrder>> _workOrderRepo;
    private readonly Mock<IRepository<WorkReport>> _workReportRepo;
    private readonly Mock<IRepository<MaterialTrace>> _materialTraceRepo;
    private readonly Mock<IRepository<AlertRecord>> _alertRepo;
    private readonly Mock<IRepository<AlertRule>> _ruleRepo;
    private readonly Mock<ILogger<QualityAlertService>> _logger;
    private readonly QualityAlertService _service;

    public QualityAlertServiceTests()
    {
        _workOrderRepo = new Mock<IRepository<WorkOrder>>();
        _workReportRepo = new Mock<IRepository<WorkReport>>();
        _materialTraceRepo = new Mock<IRepository<MaterialTrace>>();
        _alertRepo = new Mock<IRepository<AlertRecord>>();
        _ruleRepo = new Mock<IRepository<AlertRule>>();
        _logger = new Mock<ILogger<QualityAlertService>>();

        _service = new QualityAlertService(
            _workOrderRepo.Object,
            _workReportRepo.Object,
            _materialTraceRepo.Object,
            _alertRepo.Object,
            _ruleRepo.Object,
            _logger.Object);
    }

    #region AnalyzeAsync

    [Fact]
    public async Task AnalyzeAsync_ReturnsListOfAlertRecordDto()
    {
        _ruleRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<AlertRule, bool>>>()))
            .ReturnsAsync([]);
        _workOrderRepo.Setup(r => r.GetAllAsync()).ReturnsAsync([]);
        _workReportRepo.Setup(r => r.GetAllAsync()).ReturnsAsync([]);
        _materialTraceRepo.Setup(r => r.GetAllAsync()).ReturnsAsync([]);
        _alertRepo.Setup(r => r.AddRangeAsync(It.IsAny<IEnumerable<AlertRecord>>()))
            .Returns(Task.CompletedTask);
        _alertRepo.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        var result = await _service.AnalyzeAsync();

        Assert.NotNull(result);
        Assert.IsType<List<AlertRecordDto>>(result);
    }

    [Fact]
    public async Task AnalyzeAsync_UsesDefaultRules_WhenNoRulesInDb()
    {
        _ruleRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<AlertRule, bool>>>()))
            .ReturnsAsync([]);
        _workOrderRepo.Setup(r => r.GetAllAsync()).ReturnsAsync([]);
        _workReportRepo.Setup(r => r.GetAllAsync()).ReturnsAsync([]);
        _materialTraceRepo.Setup(r => r.GetAllAsync()).ReturnsAsync([]);

        var result = await _service.AnalyzeAsync();

        // Default rules exist but no data triggers alerts, so result should be empty
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task AnalyzeAsync_AcceptsWorkOrderIdFilter()
    {
        _ruleRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<AlertRule, bool>>>()))
            .ReturnsAsync([]);
        _workOrderRepo.Setup(r => r.GetAllAsync()).ReturnsAsync([]);
        _workReportRepo.Setup(r => r.GetAllAsync()).ReturnsAsync([]);
        _materialTraceRepo.Setup(r => r.GetAllAsync()).ReturnsAsync([]);

        var result = await _service.AnalyzeAsync(workOrderId: 1);

        Assert.NotNull(result);
        Assert.IsType<List<AlertRecordDto>>(result);
    }

    #endregion

    #region GetActiveAlertsAsync

    [Fact]
    public async Task GetActiveAlertsAsync_ReturnsListOfAlertRecordDto()
    {
        var alerts = new List<AlertRecord>
        {
            TestEntityFactory.CreateAlertRecordDirect(id: 1, title: "Alert 1", level: AlertLevel.High, isProcessed: false),
            TestEntityFactory.CreateAlertRecordDirect(id: 2, title: "Alert 2", level: AlertLevel.Critical, isProcessed: false),
        };

        _alertRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<AlertRecord, bool>>>()))
            .ReturnsAsync(alerts);

        var result = await _service.GetActiveAlertsAsync();

        Assert.NotNull(result);
        Assert.IsType<List<AlertRecordDto>>(result);
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetActiveAlertsAsync_ReturnsEmptyList_WhenNoActiveAlerts()
    {
        _alertRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<AlertRecord, bool>>>()))
            .ReturnsAsync([]);

        var result = await _service.GetActiveAlertsAsync();

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetActiveAlertsAsync_MapsFieldsCorrectly()
    {
        var alert = TestEntityFactory.CreateAlertRecordDirect(id: 1, title: "Test Alert", message: "Test message", level: AlertLevel.High, isProcessed: false);

        _alertRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<AlertRecord, bool>>>()))
            .ReturnsAsync(new[] { alert });

        var result = await _service.GetActiveAlertsAsync();

        Assert.Single(result);
        Assert.Equal(1, result[0].Id);
        Assert.Equal("Test Alert", result[0].Title);
        Assert.Equal((int)AlertLevel.High, result[0].Level);
        Assert.False(result[0].IsProcessed);
    }

    #endregion

    #region GetAlertHistoryAsync

    [Fact]
    public async Task GetAlertHistoryAsync_ReturnsListOfAlertRecordDto()
    {
        var alerts = new List<AlertRecord>
        {
            TestEntityFactory.CreateAlertRecordDirect(id: 1, title: "Alert 1"),
            TestEntityFactory.CreateAlertRecordDirect(id: 2, title: "Alert 2"),
            TestEntityFactory.CreateAlertRecordDirect(id: 3, title: "Alert 3"),
        };

        _alertRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<AlertRecord, bool>>>()))
            .ReturnsAsync(alerts);

        var result = await _service.GetAlertHistoryAsync(page: 1, pageSize: 2);

        Assert.NotNull(result);
        Assert.IsType<List<AlertRecordDto>>(result);
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetAlertHistoryAsync_ReturnsEmptyList_WhenNoAlerts()
    {
        _alertRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<AlertRecord, bool>>>()))
            .ReturnsAsync([]);

        var result = await _service.GetAlertHistoryAsync();

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAlertHistoryAsync_RespectsPagination()
    {
        var alerts = Enumerable.Range(1, 5)
            .Select(i => TestEntityFactory.CreateAlertRecordDirect(id: i, title: $"Alert {i}"))
            .ToList();

        _alertRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<AlertRecord, bool>>>()))
            .ReturnsAsync(alerts);

        var page1 = await _service.GetAlertHistoryAsync(page: 1, pageSize: 2);
        var page2 = await _service.GetAlertHistoryAsync(page: 2, pageSize: 2);

        Assert.Equal(2, page1.Count);
        Assert.Equal(2, page2.Count);
    }

    #endregion
}
