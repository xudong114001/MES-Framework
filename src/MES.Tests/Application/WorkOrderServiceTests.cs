using MES.Application.Services;
using MES.Domain.Entities;
using MES.Domain.Enums;
using MES.Infrastructure.Repositories;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MES.Tests.Application;

public class WorkOrderServiceTests
{
    private readonly Mock<IRepository<WorkOrder>> _workOrderRepo;
    private readonly Mock<IRepository<WorkOrderStep>> _stepRepo;
    private readonly Mock<IRepository<Material>> _materialRepo;
    private readonly Mock<IRepository<Routing>> _routingRepo;
    private readonly Mock<IRepository<Bom>> _bomRepo;
    private readonly Mock<ILogger<WorkOrderService>> _logger;
    private readonly WorkOrderService _service;

    public WorkOrderServiceTests()
    {
        _workOrderRepo = new Mock<IRepository<WorkOrder>>();
        _stepRepo = new Mock<IRepository<WorkOrderStep>>();
        _materialRepo = new Mock<IRepository<Material>>();
        _routingRepo = new Mock<IRepository<Routing>>();
        _bomRepo = new Mock<IRepository<Bom>>();
        _logger = new Mock<ILogger<WorkOrderService>>();

        _service = new WorkOrderService(
            _workOrderRepo.Object,
            _stepRepo.Object,
            _materialRepo.Object,
            _routingRepo.Object,
            _bomRepo.Object,
            _logger.Object);
    }

    private WorkOrder CreateValidWorkOrder(
        WorkOrderStatus status = WorkOrderStatus.PENDING,
        long? routingId = null,
        decimal completedQty = 0,
        decimal scrapQty = 0,
        decimal plannedQty = 100,
        string? orderNo = null,
        long? lineId = null)
    {
        return TestEntityFactory.CreateWorkOrderWithStatus(
            orderNo: orderNo ?? "WO-001",
            materialId: 100,
            plannedQty: plannedQty,
            status: status,
            completedQty: completedQty,
            scrapQty: scrapQty,
            routingId: routingId,
            lineId: lineId
        );
    }

    private Material CreateValidMaterial(long id, decimal stockQty = 1000)
    {
        return TestEntityFactory.CreateMaterial(id: id, stockQty: stockQty);
    }

    private Routing CreateTestRouting(long id, int stepCount = 3)
    {
        var routing = TestEntityFactory.CreateRouting(
            materialId: 100,
            routingCode: "R-TEST-001",
            routingName: "Test Routing");

        // 使用反射设置 Id
        TestEntityFactory.SetProperty(routing, "Id", id);

        for (int i = 1; i <= stepCount; i++)
        {
            var routingStep = TestEntityFactory.CreateRoutingStep(
                routingId: id,
                stepNo: i,
                stepName: $"Step {i}");
            routing.AddStep(routingStep);
        }

        return routing;
    }

    [Fact]
    public async Task CreateWorkOrderAsync_SetsStatusToPending()
    {
        // 创建时设置状态为 RELEASED，但服务应该重置为 PENDING
        var wo = CreateValidWorkOrder(status: WorkOrderStatus.RELEASED);

        _materialRepo.Setup(r => r.GetByIdAsync(wo.MaterialId)).ReturnsAsync(CreateValidMaterial(wo.MaterialId));
        _bomRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Bom, bool>>>()))
            .ReturnsAsync(Enumerable.Empty<Bom>());
        _workOrderRepo.Setup(r => r.AddAsync(It.IsAny<WorkOrder>())).ReturnsAsync(wo);

        var result = await _service.CreateWorkOrderAsync(wo);

        Assert.Equal(WorkOrderStatus.PENDING, result.Status);
    }

    [Fact]
    public async Task CreateWorkOrderAsync_ResetsCompletedAndScrapQty()
    {
        // 创建时设置数量，但服务应该重置为 0
        var wo = CreateValidWorkOrder(completedQty: 50, scrapQty: 10);

        _materialRepo.Setup(r => r.GetByIdAsync(wo.MaterialId)).ReturnsAsync(CreateValidMaterial(wo.MaterialId));
        _bomRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Bom, bool>>>()))
            .ReturnsAsync(Enumerable.Empty<Bom>());
        _workOrderRepo.Setup(r => r.AddAsync(It.IsAny<WorkOrder>())).ReturnsAsync(wo);

        var result = await _service.CreateWorkOrderAsync(wo);

        Assert.Equal(0, result.CompletedQty);
        Assert.Equal(0, result.ScrapQty);
    }

    [Fact]
    public async Task CreateWorkOrderAsync_ThrowsWhenMaterialNotFound()
    {
        var wo = CreateValidWorkOrder();
        _materialRepo.Setup(r => r.GetByIdAsync(wo.MaterialId)).ReturnsAsync((Material?)null);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateWorkOrderAsync(wo));
    }

    [Fact]
    public async Task CreateWorkOrderAsync_ThrowsWhenBomComponentInsufficientStock()
    {
        var wo = CreateValidWorkOrder(plannedQty: 100);

        var material = CreateValidMaterial(wo.MaterialId);
        var componentMaterial = CreateValidMaterial(200, stockQty: 5);
        var bomItem = TestEntityFactory.CreateBom(productId: wo.MaterialId, materialId: 200, quantity: 1);

        _materialRepo.Setup(r => r.GetByIdAsync(wo.MaterialId)).ReturnsAsync(material);
        _bomRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Bom, bool>>>()))
            .ReturnsAsync(new[] { bomItem }.AsEnumerable());
        _materialRepo.Setup(r => r.GetByIdAsync(200)).ReturnsAsync(componentMaterial);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateWorkOrderAsync(wo));
        Assert.Contains("库存不足", ex.Message);
    }

    [Fact]
    public async Task CreateWorkOrderAsync_GeneratesStepsWhenRoutingProvided()
    {
        var wo = CreateValidWorkOrder(routingId: 10);

        var routing = CreateTestRouting(10);

        _materialRepo.Setup(r => r.GetByIdAsync(wo.MaterialId)).ReturnsAsync(CreateValidMaterial(wo.MaterialId));
        _bomRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Bom, bool>>>()))
            .ReturnsAsync(Enumerable.Empty<Bom>());
        _workOrderRepo.Setup(r => r.AddAsync(It.IsAny<WorkOrder>())).ReturnsAsync(wo);
        _routingRepo.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(routing);

        await _service.CreateWorkOrderAsync(wo);

        _stepRepo.Verify(r => r.AddAsync(It.IsAny<WorkOrderStep>()), Times.Exactly(3));
    }

    [Fact]
    public async Task ReleaseWorkOrderAsync_TransitionsPendingToReleased()
    {
        // 初始状态为 PENDING，服务应该调用 Release() 将其变为 RELEASED
        var wo = CreateValidWorkOrder(status: WorkOrderStatus.PENDING);

        _workOrderRepo.Setup(r => r.GetByIdAsync(wo.Id)).ReturnsAsync(wo);
        _stepRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<WorkOrderStep, bool>>>()))
            .ReturnsAsync(Enumerable.Empty<WorkOrderStep>());

        await _service.ReleaseWorkOrderAsync(wo.Id);

        Assert.Equal(WorkOrderStatus.RELEASED, wo.Status);
        _workOrderRepo.Verify(r => r.UpdateAsync(wo), Times.Once);
    }

    [Fact]
    public async Task ReleaseWorkOrderAsync_ThrowsWhenNotPending()
    {
        var wo = CreateValidWorkOrder(status: WorkOrderStatus.IN_PROGRESS);

        _workOrderRepo.Setup(r => r.GetByIdAsync(wo.Id)).ReturnsAsync(wo);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.ReleaseWorkOrderAsync(wo.Id));
        Assert.Contains("不允许下达", ex.Message);
    }

    [Fact]
    public async Task HoldWorkOrderAsync_TransitionsReleasedToOnHold()
    {
        var wo = CreateValidWorkOrder(status: WorkOrderStatus.RELEASED);

        _workOrderRepo.Setup(r => r.GetByIdAsync(wo.Id)).ReturnsAsync(wo);

        await _service.HoldWorkOrderAsync(wo.Id);

        Assert.Equal(WorkOrderStatus.ON_HOLD, wo.Status);
    }

    [Fact]
    public async Task HoldWorkOrderAsync_TransitionsInProgressToOnHold()
    {
        var wo = CreateValidWorkOrder(status: WorkOrderStatus.IN_PROGRESS);

        _workOrderRepo.Setup(r => r.GetByIdAsync(wo.Id)).ReturnsAsync(wo);

        await _service.HoldWorkOrderAsync(wo.Id);

        Assert.Equal(WorkOrderStatus.ON_HOLD, wo.Status);
    }

    [Fact]
    public async Task HoldWorkOrderAsync_ThrowsWhenInvalidStatus()
    {
        var wo = CreateValidWorkOrder(status: WorkOrderStatus.COMPLETED);

        _workOrderRepo.Setup(r => r.GetByIdAsync(wo.Id)).ReturnsAsync(wo);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.HoldWorkOrderAsync(wo.Id));
    }

    [Fact]
    public async Task ResumeWorkOrderAsync_TransitionsOnHoldToInProgress()
    {
        var wo = CreateValidWorkOrder(status: WorkOrderStatus.ON_HOLD);

        _workOrderRepo.Setup(r => r.GetByIdAsync(wo.Id)).ReturnsAsync(wo);

        await _service.ResumeWorkOrderAsync(wo.Id);

        Assert.Equal(WorkOrderStatus.IN_PROGRESS, wo.Status);
    }

    [Fact]
    public async Task ResumeWorkOrderAsync_ThrowsWhenNotOnHold()
    {
        var wo = CreateValidWorkOrder(status: WorkOrderStatus.RELEASED);

        _workOrderRepo.Setup(r => r.GetByIdAsync(wo.Id)).ReturnsAsync(wo);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.ResumeWorkOrderAsync(wo.Id));
    }

    [Fact]
    public async Task CancelWorkOrderAsync_TransitionsPendingToCancelled()
    {
        var wo = CreateValidWorkOrder(status: WorkOrderStatus.PENDING);

        _workOrderRepo.Setup(r => r.GetByIdAsync(wo.Id)).ReturnsAsync(wo);

        await _service.CancelWorkOrderAsync(wo.Id);

        Assert.Equal(WorkOrderStatus.CANCELLED, wo.Status);
    }

    [Fact]
    public async Task CancelWorkOrderAsync_TransitionsReleasedToCancelled()
    {
        var wo = CreateValidWorkOrder(status: WorkOrderStatus.RELEASED);

        _workOrderRepo.Setup(r => r.GetByIdAsync(wo.Id)).ReturnsAsync(wo);

        await _service.CancelWorkOrderAsync(wo.Id);

        Assert.Equal(WorkOrderStatus.CANCELLED, wo.Status);
    }

    [Fact]
    public async Task CancelWorkOrderAsync_TransitionsOnHoldToCancelled()
    {
        var wo = CreateValidWorkOrder(status: WorkOrderStatus.ON_HOLD);

        _workOrderRepo.Setup(r => r.GetByIdAsync(wo.Id)).ReturnsAsync(wo);

        await _service.CancelWorkOrderAsync(wo.Id);

        Assert.Equal(WorkOrderStatus.CANCELLED, wo.Status);
    }

    [Fact]
    public async Task CancelWorkOrderAsync_ThrowsWhenInProgress()
    {
        var wo = CreateValidWorkOrder(status: WorkOrderStatus.IN_PROGRESS);

        _workOrderRepo.Setup(r => r.GetByIdAsync(wo.Id)).ReturnsAsync(wo);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CancelWorkOrderAsync(wo.Id));
    }

    [Fact]
    public async Task CancelWorkOrderAsync_ThrowsWhenCompleted()
    {
        var wo = CreateValidWorkOrder(status: WorkOrderStatus.COMPLETED);

        _workOrderRepo.Setup(r => r.GetByIdAsync(wo.Id)).ReturnsAsync(wo);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CancelWorkOrderAsync(wo.Id));
    }

    [Fact]
    public async Task CloseWorkOrderAsync_TransitionsCompletedToClosed()
    {
        var wo = CreateValidWorkOrder(status: WorkOrderStatus.COMPLETED);

        _workOrderRepo.Setup(r => r.GetByIdAsync(wo.Id)).ReturnsAsync(wo);

        await _service.CloseWorkOrderAsync(wo.Id);

        Assert.Equal(WorkOrderStatus.CLOSED, wo.Status);
        Assert.NotNull(wo.ActualEndTime);
    }

    [Fact]
    public async Task CloseWorkOrderAsync_ThrowsWhenNotCompleted()
    {
        var wo = CreateValidWorkOrder(status: WorkOrderStatus.IN_PROGRESS);

        _workOrderRepo.Setup(r => r.GetByIdAsync(wo.Id)).ReturnsAsync(wo);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CloseWorkOrderAsync(wo.Id));
    }

    [Fact]
    public async Task SplitWorkOrderAsync_SplitsCorrectly()
    {
        var wo = CreateValidWorkOrder(status: WorkOrderStatus.PENDING, plannedQty: 100, orderNo: "WO-001");

        _workOrderRepo.Setup(r => r.GetByIdAsync(wo.Id)).ReturnsAsync(wo);
        _workOrderRepo.Setup(r => r.AddAsync(It.IsAny<WorkOrder>()))
            .ReturnsAsync((WorkOrder w) => { TestEntityFactory.SetProperty(w, "Id", 2); return w; });
        _bomRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Bom, bool>>>()))
            .ReturnsAsync(Enumerable.Empty<Bom>());

        var child = await _service.SplitWorkOrderAsync(wo.Id, 30);

        Assert.Equal(70, wo.PlannedQty);
        Assert.Equal(30, child.PlannedQty);
        Assert.Equal("WO-001-SUB", child.OrderNo);
        Assert.Equal(WorkOrderStatus.PENDING, child.Status);
    }

    [Fact]
    public async Task SplitWorkOrderAsync_ThrowsWhenSplitQtyZero()
    {
        var wo = CreateValidWorkOrder(status: WorkOrderStatus.PENDING, plannedQty: 100);

        _workOrderRepo.Setup(r => r.GetByIdAsync(wo.Id)).ReturnsAsync(wo);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.SplitWorkOrderAsync(wo.Id, 0));
    }

    [Fact]
    public async Task SplitWorkOrderAsync_ThrowsWhenSplitQtyExceedsPlanned()
    {
        var wo = CreateValidWorkOrder(status: WorkOrderStatus.PENDING, plannedQty: 100);

        _workOrderRepo.Setup(r => r.GetByIdAsync(wo.Id)).ReturnsAsync(wo);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.SplitWorkOrderAsync(wo.Id, 100));
    }

    [Fact]
    public async Task SplitWorkOrderAsync_ThrowsWhenInvalidStatus()
    {
        var wo = CreateValidWorkOrder(status: WorkOrderStatus.IN_PROGRESS, plannedQty: 100);

        _workOrderRepo.Setup(r => r.GetByIdAsync(wo.Id)).ReturnsAsync(wo);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.SplitWorkOrderAsync(wo.Id, 30));
    }

    [Fact]
    public async Task ReworkWorkOrderAsync_CreatesReworkChildOrder()
    {
        var wo = CreateValidWorkOrder(status: WorkOrderStatus.COMPLETED, completedQty: 50, orderNo: "WO-001");

        _workOrderRepo.Setup(r => r.GetByIdAsync(wo.Id)).ReturnsAsync(wo);
        _workOrderRepo.Setup(r => r.AddAsync(It.IsAny<WorkOrder>()))
            .ReturnsAsync((WorkOrder w) => { TestEntityFactory.SetProperty(w, "Id", 2); return w; });

        var child = await _service.ReworkWorkOrderAsync(wo.Id, 20, "quality issue");

        Assert.Equal(30, wo.CompletedQty);
        Assert.Equal(20, child.PlannedQty);
        Assert.Equal("WO-001-RWK", child.OrderNo);
        Assert.Equal(WorkOrderStatus.PENDING, child.Status);
        Assert.Equal(wo.Id, child.ReworkFromId);
    }

    [Fact]
    public async Task ReworkWorkOrderAsync_ThrowsWhenReworkQtyExceedsCompleted()
    {
        var wo = CreateValidWorkOrder(status: WorkOrderStatus.COMPLETED, completedQty: 10);

        _workOrderRepo.Setup(r => r.GetByIdAsync(wo.Id)).ReturnsAsync(wo);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.ReworkWorkOrderAsync(wo.Id, 20, null));
    }

    [Fact]
    public async Task ReworkWorkOrderAsync_ThrowsWhenInvalidStatus()
    {
        var wo = CreateValidWorkOrder(status: WorkOrderStatus.PENDING, completedQty: 50);

        _workOrderRepo.Setup(r => r.GetByIdAsync(wo.Id)).ReturnsAsync(wo);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.ReworkWorkOrderAsync(wo.Id, 10, null));
    }

    [Fact]
    public async Task ScrapWorkOrderAsync_AddsScrapQty()
    {
        var wo = CreateValidWorkOrder(
            status: WorkOrderStatus.IN_PROGRESS,
            plannedQty: 100,
            completedQty: 40,
            scrapQty: 10);

        _workOrderRepo.Setup(r => r.GetByIdAsync(wo.Id)).ReturnsAsync(wo);

        await _service.ScrapWorkOrderAsync(wo.Id, 20, "defective");

        Assert.Equal(30, wo.ScrapQty);
        Assert.Equal("defective", wo.Remark);
    }

    [Fact]
    public async Task ScrapWorkOrderAsync_CancelsWhenFullyScrapped()
    {
        var wo = CreateValidWorkOrder(
            status: WorkOrderStatus.IN_PROGRESS,
            plannedQty: 100,
            completedQty: 30,
            scrapQty: 0);

        _workOrderRepo.Setup(r => r.GetByIdAsync(wo.Id)).ReturnsAsync(wo);

        await _service.ScrapWorkOrderAsync(wo.Id, 70, null);

        Assert.Equal(WorkOrderStatus.CANCELLED, wo.Status);
    }

    [Fact]
    public async Task ScrapWorkOrderAsync_ThrowsWhenScrapExceedsRemaining()
    {
        var wo = CreateValidWorkOrder(
            status: WorkOrderStatus.IN_PROGRESS,
            plannedQty: 100,
            completedQty: 80,
            scrapQty: 10);

        _workOrderRepo.Setup(r => r.GetByIdAsync(wo.Id)).ReturnsAsync(wo);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.ScrapWorkOrderAsync(wo.Id, 20, null));
        Assert.Contains("超过剩余可操作数量", ex.Message);
    }

    [Fact]
    public async Task ScrapWorkOrderAsync_ThrowsWhenInvalidStatus()
    {
        var wo = CreateValidWorkOrder(status: WorkOrderStatus.PENDING);

        _workOrderRepo.Setup(r => r.GetByIdAsync(wo.Id)).ReturnsAsync(wo);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.ScrapWorkOrderAsync(wo.Id, 10, null));
    }

    [Fact]
    public async Task ReleaseWorkOrderAsync_ThrowsWhenWorkOrderNotFound()
    {
        _workOrderRepo.Setup(r => r.GetByIdAsync(It.IsAny<long>())).ReturnsAsync((WorkOrder?)null);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.ReleaseWorkOrderAsync(999));
    }

    [Fact]
    public async Task HoldWorkOrderAsync_ThrowsWhenWorkOrderNotFound()
    {
        _workOrderRepo.Setup(r => r.GetByIdAsync(It.IsAny<long>())).ReturnsAsync((WorkOrder?)null);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.HoldWorkOrderAsync(999));
    }

    [Fact]
    public async Task ReworkWorkOrderAsync_WorksWithInProgressStatus()
    {
        var wo = CreateValidWorkOrder(
            status: WorkOrderStatus.IN_PROGRESS,
            completedQty: 30,
            orderNo: "WO-002");

        _workOrderRepo.Setup(r => r.GetByIdAsync(wo.Id)).ReturnsAsync(wo);
        _workOrderRepo.Setup(r => r.AddAsync(It.IsAny<WorkOrder>()))
            .ReturnsAsync((WorkOrder w) => { TestEntityFactory.SetProperty(w, "Id", 3); return w; });

        var child = await _service.ReworkWorkOrderAsync(wo.Id, 10, null);

        Assert.Equal(20, wo.CompletedQty);
        Assert.Equal(10, child.PlannedQty);
        Assert.Equal("WO-002-RWK", child.OrderNo);
    }
}