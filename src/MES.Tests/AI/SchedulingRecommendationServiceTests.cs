using MES.AI.Application.Dtos;
using MES.AI.Application.Services;
using MES.Domain.Entities;
using MES.Domain.Enums;
using MES.Domain.Repositories;
using MES.Tests;
using Moq;
using Xunit;

namespace MES.Tests.AI;

public class SchedulingRecommendationServiceTests
{
    private readonly Mock<IRepository<WorkOrder>> _workOrderRepo;
    private readonly Mock<IRepository<ProductionLine>> _lineRepo;
    private readonly Mock<IRepository<WorkReport>> _reportRepo;
    private readonly Mock<IRepository<RoutingStep>> _routingStepRepo;
    private readonly Mock<IRepository<Workstation>> _workstationRepo;
    private readonly SchedulingRecommendationService _service;

    public SchedulingRecommendationServiceTests()
    {
        _workOrderRepo = new Mock<IRepository<WorkOrder>>();
        _lineRepo = new Mock<IRepository<ProductionLine>>();
        _reportRepo = new Mock<IRepository<WorkReport>>();
        _routingStepRepo = new Mock<IRepository<RoutingStep>>();
        _workstationRepo = new Mock<IRepository<Workstation>>();

        _service = new SchedulingRecommendationService(
            _workOrderRepo.Object,
            _lineRepo.Object,
            _reportRepo.Object,
            _routingStepRepo.Object,
            _workstationRepo.Object);
    }

    private ProductionLine CreateLine(long id, string name, bool status)
    {
        var line = ProductionLine.Create($"LINE-{id}", name, 1, LineType.FLOW);
        TestEntityFactory.SetProperty(line, "Id", id);
        TestEntityFactory.SetProperty(line, "Status", status);
        return line;
    }

    [Fact]
    public async Task GetRecommendationsAsync_ReturnsEmpty_WhenNoActiveLines()
    {
        var workOrder = TestEntityFactory.CreateWorkOrderDirect(id: 1, orderNo: "WO-001", materialId: 1);
        _workOrderRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(workOrder);
        _lineRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<ProductionLine, bool>>>()))
            .ReturnsAsync(new List<ProductionLine>().AsEnumerable());

        var result = await _service.GetRecommendationsAsync(1);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetRecommendationsAsync_Throws_WhenWorkOrderNotFound()
    {
        _workOrderRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((WorkOrder?)null);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.GetRecommendationsAsync(999));
    }

    [Fact]
    public async Task GetRecommendationsAsync_ReturnsResults_WhenValidInput()
    {
        var workOrder = TestEntityFactory.CreateWorkOrderDirect(
            id: 1,
            orderNo: "WO-001",
            materialId: 1,
            routingId: 1,
            plannedQty: 100,
            status: WorkOrderStatus.PENDING
        );
        TestEntityFactory.SetProperty(workOrder, "PlanStartTime", DateTime.UtcNow);
        TestEntityFactory.SetProperty(workOrder, "PlanEndTime", DateTime.UtcNow.AddDays(5));

        var lines = new List<ProductionLine>
        {
            CreateLine(1, "Line1", true),
            CreateLine(2, "Line2", true)
        };

        _workOrderRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(workOrder);
        _lineRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<ProductionLine, bool>>>()))
            .ReturnsAsync(lines.AsEnumerable());
        _workOrderRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<WorkOrder, bool>>>()))
            .ReturnsAsync(new List<WorkOrder>().AsEnumerable());
        _routingStepRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<RoutingStep, bool>>>()))
            .ReturnsAsync(new List<RoutingStep>().AsEnumerable());
        _workstationRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Workstation, bool>>>()))
            .ReturnsAsync(new List<Workstation>().AsEnumerable());

        var result = await _service.GetRecommendationsAsync(1);

        Assert.NotNull(result);
        Assert.True(result.Count <= 3);
    }
}
