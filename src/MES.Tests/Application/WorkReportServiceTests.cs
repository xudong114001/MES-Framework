using MES.Application.Services;
using MES.Application.Interfaces;
using MES.Domain.Entities;
using MES.Domain.Enums;
using MES.Domain.Exceptions;
using MES.Domain.Repositories;
using MES.Tests;
using Moq;
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
    private readonly Mock<ICacheService> _cacheService;
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
        _cacheService = new Mock<ICacheService>();
        _cacheService.Setup(c => c.SetIfNotExistsAsync(It.IsAny<string>(), It.IsAny<TimeSpan>()))
            .ReturnsAsync(true);

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
            _cacheService.Object);
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
            status: WorkOrderStatus.RELEASED,
            lineId: 1);
    }

    private WorkReport CreateValidReport(long workOrderId = 1, decimal goodQty = 10, decimal scrapQty = 0, decimal reworkQty = 0)
    {
        return TestEntityFactory.CreateWorkReportDirect(
            id: 1,
            workOrderId: workOrderId,
            goodQty: goodQty,
            scrapQty: scrapQty,
            reworkQty: reworkQty,
            reportType: ReportType.COMPLETE
        );
    }

    [Fact]
    public async Task SubmitReportAsync_ThrowsWhenWorkOrderNotFound()
    {
        var report = CreateValidReport(workOrderId: 999);

        _workOrderRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((WorkOrder?)null);

        await Assert.ThrowsAsync<DomainException>(() => _service.SubmitReportAsync(report));
    }

    [Fact]
    public async Task SubmitReportAsync_ThrowsWhenWorkOrderStatusInvalid()
    {
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

        _workOrderRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(wo);

        await Assert.ThrowsAsync<DomainException>(() => _service.SubmitReportAsync(report));
    }

    [Fact]
    public async Task SubmitReportAsync_UpdatesWorkOrderCompletedQty()
    {
        var report = CreateValidReport(goodQty: 30);
        var wo = CreateReleasedWorkOrder();

        _workOrderRepo.Setup(r => r.GetByIdAsync(report.WorkOrderId)).ReturnsAsync(wo);
        _checkpointRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<QcCheckpoint, bool>>>()))
            .ReturnsAsync(Enumerable.Empty<QcCheckpoint>());
        _reportRepo.Setup(r => r.AddAsync(It.IsAny<WorkReport>())).ReturnsAsync(report);
        _workOrderRepo.Setup(r => r.UpdateAsync(It.IsAny<WorkOrder>())).Returns(Task.CompletedTask);

        await _service.SubmitReportAsync(report);

        _workOrderRepo.Verify(r => r.UpdateAsync(It.IsAny<WorkOrder>()), Times.Once);
    }
}
