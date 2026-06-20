using MES.Application.Services;
using MES.Domain.Entities;
using MES.Domain.Enums;
using MES.Infrastructure.Repositories;
using Moq;
using StackExchange.Redis;
using Xunit;

namespace MES.Tests.Application;

public class WorkReportServiceTests
{
    private readonly Mock<IRepository<WorkReport>> _reportRepo;
    private readonly Mock<IRepository<WorkOrder>> _workOrderRepo;
    private readonly Mock<IRepository<WorkOrderStep>> _stepRepo;
    private readonly Mock<IRepository<Workstation>> _workstationRepo;
    private readonly Mock<IRepository<User>> _userRepo;
    private readonly Mock<IRepository<QcCheckpoint>> _checkpointRepo;
    private readonly Mock<IRepository<QcInspection>> _inspectionRepo;
    private readonly Mock<IConnectionMultiplexer> _redisConn;
    private readonly Mock<IDatabase> _redisDb;
    private readonly WorkReportService _service;
    private readonly List<QcInspection> _inspectionData = new();
    private readonly List<QcCheckpoint> _checkpointData = new();

    public WorkReportServiceTests()
    {
        _reportRepo = new Mock<IRepository<WorkReport>>();
        _workOrderRepo = new Mock<IRepository<WorkOrder>>();
        _stepRepo = new Mock<IRepository<WorkOrderStep>>();
        _workstationRepo = new Mock<IRepository<Workstation>>();
        _userRepo = new Mock<IRepository<User>>();
        _checkpointRepo = new Mock<IRepository<QcCheckpoint>>();
        _inspectionRepo = new Mock<IRepository<QcInspection>>();
        _redisConn = new Mock<IConnectionMultiplexer>();
        _redisDb = new Mock<IDatabase>();

        _redisConn.Setup(c => c.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(_redisDb.Object);

        _checkpointRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<QcCheckpoint, bool>>>()))
            .Returns((System.Linq.Expressions.Expression<Func<QcCheckpoint, bool>> expr) =>
            {
                var compiled = expr.Compile();
                return Task.FromResult(_checkpointData.Where(compiled).AsEnumerable());
            });

        _inspectionRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<QcInspection, bool>>>()))
            .Returns((System.Linq.Expressions.Expression<Func<QcInspection, bool>> expr) =>
            {
                var compiled = expr.Compile();
                return Task.FromResult(_inspectionData.Where(compiled).AsEnumerable());
            });

        _service = new WorkReportService(
            _reportRepo.Object,
            _workOrderRepo.Object,
            _stepRepo.Object,
            _workstationRepo.Object,
            _userRepo.Object,
            _checkpointRepo.Object,
            _inspectionRepo.Object,
            _redisConn.Object);
    }

    private WorkOrder CreateReleasedWorkOrder(decimal plannedQty = 100, decimal completedQty = 0, decimal scrapQty = 0)
    {
        return TestEntityFactory.CreateWorkOrderDirect(
            id: 1,
            orderNo: "WO-001",
            materialId: 1,
            plannedQty: plannedQty,
            completedQty: completedQty,
            scrapQty: scrapQty,
            status: WorkOrderStatus.RELEASED
        );
    }

    private WorkReport CreateValidReport(long workOrderId = 1, decimal goodQty = 10, decimal scrapQty = 0, decimal reworkQty = 0)
    {
        return TestEntityFactory.CreateWorkReportDirect(
            workOrderId: workOrderId,
            goodQty: goodQty,
            scrapQty: scrapQty,
            reworkQty: reworkQty,
            reportType: ReportType.COMPLETE
        );
    }

    private void SetupRedisLockSuccess()
    {
        _redisDb.Setup(db => db.StringSetAsync(
            It.IsAny<RedisKey>(), It.IsAny<RedisValue>(),
            It.IsAny<TimeSpan?>(), It.IsAny<When>()))
            .ReturnsAsync(true);
        _redisDb.Setup(db => db.StringSetAsync(
            It.IsAny<RedisKey>(), It.IsAny<RedisValue>(),
            It.IsAny<TimeSpan?>(), It.IsAny<When>(),
            It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);
    }

    private void SetupRedisLockFail()
    {
        _redisDb.Setup(db => db.StringSetAsync(
            It.IsAny<RedisKey>(), It.IsAny<RedisValue>(),
            It.IsAny<TimeSpan?>(), It.IsAny<When>()))
            .ReturnsAsync(false);
        _redisDb.Setup(db => db.StringSetAsync(
            It.IsAny<RedisKey>(), It.IsAny<RedisValue>(),
            It.IsAny<TimeSpan?>(), It.IsAny<When>(),
            It.IsAny<CommandFlags>()))
            .ReturnsAsync(false);
    }

    [Fact]
    public async Task SubmitReportAsync_ThrowsWhenDuplicateSubmission()
    {
        SetupRedisLockFail();
        var report = CreateValidReport();

        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.SubmitReportAsync(report));
    }

    [Fact]
    public async Task SubmitReportAsync_ThrowsWhenWorkOrderNotFound()
    {
        SetupRedisLockSuccess();
        var report = CreateValidReport(workOrderId: 999);

        _workOrderRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((WorkOrder?)null);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.SubmitReportAsync(report));
    }

    [Fact]
    public async Task SubmitReportAsync_ThrowsWhenWorkOrderStatusInvalid()
    {
        SetupRedisLockSuccess();
        var report = CreateValidReport();
        var wo = TestEntityFactory.CreateWorkOrderDirect(
            id: 1,
            orderNo: "WO-001",
            materialId: 1,
            plannedQty: 100,
            completedQty: 0,
            scrapQty: 0,
            status: WorkOrderStatus.PENDING
        );

        _workOrderRepo.Setup(r => r.GetByIdAsync(report.WorkOrderId)).ReturnsAsync(wo);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.SubmitReportAsync(report));
        Assert.Contains("不允许报工", ex.Message);
    }

    [Fact]
    public async Task SubmitReportAsync_ThrowsWhenReportQtyExceedsRemaining()
    {
        SetupRedisLockSuccess();
        var report = CreateValidReport(goodQty: 80, scrapQty: 30);
        var wo = CreateReleasedWorkOrder(plannedQty: 100);

        _workOrderRepo.Setup(r => r.GetByIdAsync(report.WorkOrderId)).ReturnsAsync(wo);
        _checkpointRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<QcCheckpoint, bool>>>()))
            .ReturnsAsync(Enumerable.Empty<QcCheckpoint>());

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.SubmitReportAsync(report));
        Assert.Contains("超过剩余可报工数量", ex.Message);
    }

    [Fact]
    public async Task SubmitReportAsync_UpdatesWorkOrderCompletedQty()
    {
        SetupRedisLockSuccess();
        var report = CreateValidReport(goodQty: 30);
        var wo = CreateReleasedWorkOrder();

        _workOrderRepo.Setup(r => r.GetByIdAsync(report.WorkOrderId)).ReturnsAsync(wo);
        _checkpointRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<QcCheckpoint, bool>>>()))
            .ReturnsAsync(Enumerable.Empty<QcCheckpoint>());
        _redisDb.Setup(db => db.StringIncrementAsync(It.IsAny<RedisKey>(), It.IsAny<long>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(1);
        _redisDb.Setup(db => db.KeyExpireAsync(It.IsAny<RedisKey>(), It.IsAny<TimeSpan?>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);
        _reportRepo.Setup(r => r.AddAsync(It.IsAny<WorkReport>())).ReturnsAsync(report);

        await _service.SubmitReportAsync(report);

        Assert.Equal(30, wo.CompletedQty);
        Assert.Equal(WorkOrderStatus.IN_PROGRESS, wo.Status);
        _workOrderRepo.Verify(r => r.UpdateAsync(wo), Times.Once);
    }

    [Fact]
    public async Task SubmitReportAsync_MarksCompletedWhenFullyReported()
    {
        SetupRedisLockSuccess();
        var report = CreateValidReport(goodQty: 100);
        var wo = CreateReleasedWorkOrder(plannedQty: 100);

        _workOrderRepo.Setup(r => r.GetByIdAsync(report.WorkOrderId)).ReturnsAsync(wo);
        _checkpointRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<QcCheckpoint, bool>>>()))
            .ReturnsAsync(Enumerable.Empty<QcCheckpoint>());
        _redisDb.Setup(db => db.StringIncrementAsync(It.IsAny<RedisKey>(), It.IsAny<long>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(1);
        _redisDb.Setup(db => db.KeyExpireAsync(It.IsAny<RedisKey>(), It.IsAny<TimeSpan?>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);
        _reportRepo.Setup(r => r.AddAsync(It.IsAny<WorkReport>())).ReturnsAsync(report);

        await _service.SubmitReportAsync(report);

        Assert.Equal(WorkOrderStatus.COMPLETED, wo.Status);
    }

    [Fact]
    public async Task SubmitReportAsync_UpdatesStepProgress_WhenStepIdProvided()
    {
        SetupRedisLockSuccess();
        var step = TestEntityFactory.CreateWorkOrderStepDirect(
            id: 10,
            workOrderId: 1,
            stepNo: 1,
            stepName: "Step 1",
            plannedQty: 100,
            completedQty: 0,
            scrapQty: 0,
            status: WorkOrderStatus.RELEASED
        );
        var report = CreateValidReport(goodQty: 50);
        TestEntityFactory.SetProperty(report, "StepId", 10);
        var wo = CreateReleasedWorkOrder();

        _workOrderRepo.Setup(r => r.GetByIdAsync(report.WorkOrderId)).ReturnsAsync(wo);
        _stepRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<WorkOrderStep, bool>>>()))
            .ReturnsAsync(new[] { step }.AsEnumerable());
        _checkpointRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<QcCheckpoint, bool>>>()))
            .ReturnsAsync(Enumerable.Empty<QcCheckpoint>());
        _redisDb.Setup(db => db.StringIncrementAsync(It.IsAny<RedisKey>(), It.IsAny<long>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(1);
        _redisDb.Setup(db => db.KeyExpireAsync(It.IsAny<RedisKey>(), It.IsAny<TimeSpan?>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);
        _reportRepo.Setup(r => r.AddAsync(It.IsAny<WorkReport>())).ReturnsAsync(report);

        await _service.SubmitReportAsync(report);

        Assert.Equal(50, step.CompletedQty);
        Assert.Equal(WorkOrderStatus.IN_PROGRESS, step.Status);
        _stepRepo.Verify(r => r.UpdateAsync(step), Times.Once);
    }

    [Fact]
    public async Task SubmitReportAsync_MarksStepCompletedWhenFullyReported()
    {
        SetupRedisLockSuccess();
        var step = TestEntityFactory.CreateWorkOrderStepDirect(
            id: 10,
            workOrderId: 1,
            stepNo: 1,
            stepName: "Step 1",
            plannedQty: 100,
            completedQty: 0,
            scrapQty: 0,
            status: WorkOrderStatus.RELEASED
        );
        var report = CreateValidReport(goodQty: 100);
        TestEntityFactory.SetProperty(report, "StepId", 10);
        var wo = CreateReleasedWorkOrder(plannedQty: 100);

        _workOrderRepo.Setup(r => r.GetByIdAsync(report.WorkOrderId)).ReturnsAsync(wo);
        _stepRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<WorkOrderStep, bool>>>()))
            .ReturnsAsync(new[] { step }.AsEnumerable());
        _checkpointRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<QcCheckpoint, bool>>>()))
            .ReturnsAsync(Enumerable.Empty<QcCheckpoint>());
        _redisDb.Setup(db => db.StringIncrementAsync(It.IsAny<RedisKey>(), It.IsAny<long>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(1);
        _redisDb.Setup(db => db.KeyExpireAsync(It.IsAny<RedisKey>(), It.IsAny<TimeSpan?>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);
        _reportRepo.Setup(r => r.AddAsync(It.IsAny<WorkReport>())).ReturnsAsync(report);

        await _service.SubmitReportAsync(report);

        Assert.Equal(WorkOrderStatus.COMPLETED, step.Status);
    }

    [Fact]
    public async Task SubmitReportAsync_ThrowsWhenMandatoryQcCheckpointPending()
    {
        SetupRedisLockSuccess();
        var report = CreateValidReport(goodQty: 10);
        TestEntityFactory.SetProperty(report, "StepId", 10);
        var wo = CreateReleasedWorkOrder();

        var checkpoint = TestEntityFactory.CreateQcCheckpointDirect(stepId: 10, checkType: QcInspectionType.FIRST, isMandatory: true);
        var pendingInspection = TestEntityFactory.CreateQcInspectionDirect(
            workOrderId: 1,
            sourceType: QcInspectionType.FIRST,
            inspectResult: QcResult.PENDING);

        _checkpointData.Clear();
        _checkpointData.Add(checkpoint);
        _inspectionData.Clear();
        _inspectionData.Add(pendingInspection);

        _workOrderRepo.Setup(r => r.GetByIdAsync(report.WorkOrderId)).ReturnsAsync(wo);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.SubmitReportAsync(report));
        Assert.Contains("需先完成质检", ex.Message);
    }

    [Fact]
    public async Task SubmitReportAsync_PassesWhenMandatoryQcCheckpointCompleted()
    {
        SetupRedisLockSuccess();
        var report = CreateValidReport(goodQty: 10);
        TestEntityFactory.SetProperty(report, "StepId", 10);
        var wo = CreateReleasedWorkOrder();

        var checkpoint = TestEntityFactory.CreateQcCheckpointDirect(stepId: 10, checkType: QcInspectionType.FIRST, isMandatory: true);
        var passedInspection = TestEntityFactory.CreateQcInspectionDirect(
            workOrderId: 1,
            sourceType: QcInspectionType.FIRST,
            inspectResult: QcResult.PASS);

        _checkpointData.Clear();
        _checkpointData.Add(checkpoint);
        _inspectionData.Clear();
        _inspectionData.Add(passedInspection);

        _workOrderRepo.Setup(r => r.GetByIdAsync(report.WorkOrderId)).ReturnsAsync(wo);
        _redisDb.Setup(db => db.StringIncrementAsync(It.IsAny<RedisKey>(), It.IsAny<long>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(1);
        _redisDb.Setup(db => db.KeyExpireAsync(It.IsAny<RedisKey>(), It.IsAny<TimeSpan?>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);
        _reportRepo.Setup(r => r.AddAsync(It.IsAny<WorkReport>())).ReturnsAsync(report);

        await _service.SubmitReportAsync(report);

        Assert.Equal(10, wo.CompletedQty);
    }

    [Fact]
    public async Task SubmitReportAsync_PassesWhenNoMandatoryQcCheckpoint()
    {
        SetupRedisLockSuccess();
        var report = CreateValidReport(goodQty: 10);
        report.StepId = 10;
        var wo = CreateReleasedWorkOrder();

        _workOrderRepo.Setup(r => r.GetByIdAsync(report.WorkOrderId)).ReturnsAsync(wo);
        _checkpointRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<QcCheckpoint, bool>>>()))
            .ReturnsAsync(Enumerable.Empty<QcCheckpoint>());
        _redisDb.Setup(db => db.StringIncrementAsync(It.IsAny<RedisKey>(), It.IsAny<long>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(1);
        _redisDb.Setup(db => db.KeyExpireAsync(It.IsAny<RedisKey>(), It.IsAny<TimeSpan?>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);
        _reportRepo.Setup(r => r.AddAsync(It.IsAny<WorkReport>())).ReturnsAsync(report);

        await _service.SubmitReportAsync(report);

        Assert.Equal(10, wo.CompletedQty);
    }

    [Fact]
    public async Task SubmitReportAsync_AccumulatesScrapQty()
    {
        SetupRedisLockSuccess();
        var report = CreateValidReport(goodQty: 30, scrapQty: 5);
        var wo = CreateReleasedWorkOrder(plannedQty: 100, completedQty: 50, scrapQty: 10);

        _workOrderRepo.Setup(r => r.GetByIdAsync(report.WorkOrderId)).ReturnsAsync(wo);
        _checkpointRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<QcCheckpoint, bool>>>()))
            .ReturnsAsync(Enumerable.Empty<QcCheckpoint>());
        _redisDb.Setup(db => db.StringIncrementAsync(It.IsAny<RedisKey>(), It.IsAny<long>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(1);
        _redisDb.Setup(db => db.KeyExpireAsync(It.IsAny<RedisKey>(), It.IsAny<TimeSpan?>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);
        _reportRepo.Setup(r => r.AddAsync(It.IsAny<WorkReport>())).ReturnsAsync(report);

        await _service.SubmitReportAsync(report);

        Assert.Equal(80, wo.CompletedQty);
        Assert.Equal(15, wo.ScrapQty);
    }

    [Fact]
    public async Task SubmitReportAsync_AllowsReportForInProgressWorkOrder()
    {
        SetupRedisLockSuccess();
        var report = CreateValidReport(goodQty: 10);
        var wo = TestEntityFactory.CreateWorkOrderDirect(
            id: 1,
            orderNo: "WO-001",
            materialId: 1,
            plannedQty: 100,
            completedQty: 0,
            scrapQty: 0,
            status: WorkOrderStatus.IN_PROGRESS
        );

        _workOrderRepo.Setup(r => r.GetByIdAsync(report.WorkOrderId)).ReturnsAsync(wo);
        _checkpointRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<QcCheckpoint, bool>>>()))
            .ReturnsAsync(Enumerable.Empty<QcCheckpoint>());
        _redisDb.Setup(db => db.StringIncrementAsync(It.IsAny<RedisKey>(), It.IsAny<long>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(1);
        _redisDb.Setup(db => db.KeyExpireAsync(It.IsAny<RedisKey>(), It.IsAny<TimeSpan?>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);
        _reportRepo.Setup(r => r.AddAsync(It.IsAny<WorkReport>())).ReturnsAsync(report);

        await _service.SubmitReportAsync(report);

        Assert.Equal(10, wo.CompletedQty);
    }

    [Fact]
    public async Task SubmitReportAsync_GeneratesBatchNo_WhenGoodQtyPositive()
    {
        SetupRedisLockSuccess();
        var report = CreateValidReport(goodQty: 10);
        var wo = CreateReleasedWorkOrder();

        _workOrderRepo.Setup(r => r.GetByIdAsync(report.WorkOrderId)).ReturnsAsync(wo);
        _checkpointRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<QcCheckpoint, bool>>>()))
            .ReturnsAsync(Enumerable.Empty<QcCheckpoint>());
        _redisDb.Setup(db => db.StringIncrementAsync(It.IsAny<RedisKey>(), It.IsAny<long>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(5);
        _redisDb.Setup(db => db.KeyExpireAsync(It.IsAny<RedisKey>(), It.IsAny<TimeSpan?>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);
        _reportRepo.Setup(r => r.AddAsync(It.IsAny<WorkReport>())).ReturnsAsync(report);

        await _service.SubmitReportAsync(report);

        Assert.NotNull(report.BatchNo);
        Assert.StartsWith("BAT", report.BatchNo);
    }

    [Fact]
    public async Task SubmitReportAsync_NoBatchNo_WhenGoodQtyZero()
    {
        SetupRedisLockSuccess();
        var report = CreateValidReport(goodQty: 0, scrapQty: 5);
        var wo = CreateReleasedWorkOrder();

        _workOrderRepo.Setup(r => r.GetByIdAsync(report.WorkOrderId)).ReturnsAsync(wo);
        _checkpointRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<QcCheckpoint, bool>>>()))
            .ReturnsAsync(Enumerable.Empty<QcCheckpoint>());
        _reportRepo.Setup(r => r.AddAsync(It.IsAny<WorkReport>())).ReturnsAsync(report);

        await _service.SubmitReportAsync(report);

        Assert.Null(report.BatchNo);
    }
}
