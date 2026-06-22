using MES.Application.Services;
using MES.Domain.Entities;
using MES.Domain.Enums;
using MES.Domain.Repositories;
using Moq;
using Xunit;

namespace MES.Tests.Application;

public class DispatchServiceTests
{
    private readonly Mock<IRepository<WorkOrder>> _workOrderRepo;
    private readonly Mock<IRepository<WorkOrderStep>> _stepRepo;
    private readonly Mock<IRepository<Workstation>> _workstationRepo;
    private readonly Mock<IRepository<ProductionLine>> _lineRepo;
    private readonly DispatchService _service;

    public DispatchServiceTests()
    {
        _workOrderRepo = new Mock<IRepository<WorkOrder>>();
        _stepRepo = new Mock<IRepository<WorkOrderStep>>();
        _workstationRepo = new Mock<IRepository<Workstation>>();
        _lineRepo = new Mock<IRepository<ProductionLine>>();

        _service = new DispatchService(
            _workOrderRepo.Object,
            _stepRepo.Object,
            _workstationRepo.Object,
            _lineRepo.Object);
    }

    private WorkOrderStep CreateStep(long id, long workOrderId, long? workstationId = null, WorkOrderStatus status = WorkOrderStatus.SCHEDULED)
    {
        return TestEntityFactory.CreateWorkOrderStepDirect(
            id: id,
            workOrderId: workOrderId,
            stepNo: 1,
            stepName: $"Step {id}",
            plannedQty: 100,
            workstationId: workstationId,
            status: status
        );
    }

    private Workstation CreateWorkstation(long id, long lineId, bool status = true)
    {
        return new Workstation
        {
            Id = id,
            Code = $"WS-{id}",
            Name = $"工位 {id}",
            LineId = lineId,
            Status = status,
            SeqNo = (int)id
        };
    }

    private WorkOrder CreateWorkOrder(long id, WorkOrderStatus status = WorkOrderStatus.SCHEDULED, Priority priority = Priority.NORMAL)
    {
        return TestEntityFactory.CreateWorkOrderWithStatus(
            orderNo: $"WO-{id:D3}",
            materialId: 1,
            plannedQty: 100,
            status: status,
            priority: priority
        );
    }

    [Fact]
    public async Task DispatchStepAsync_DispatchesSuccessfully()
    {
        var step = CreateStep(1, 100);
        var ws = CreateWorkstation(10, 1);

        _stepRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(step);
        _workstationRepo.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(ws);
        _stepRepo.Setup(r => r.UpdateAsync(It.IsAny<WorkOrderStep>())).Returns(Task.CompletedTask);

        await _service.DispatchStepAsync(1, 10);

        _stepRepo.Verify(r => r.UpdateAsync(It.Is<WorkOrderStep>(s =>
            s.WorkstationId == 10)), Times.Once);
    }

    [Fact]
    public async Task DispatchStepAsync_ThrowsWhenStepNotFound()
    {
        _stepRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((WorkOrderStep?)null);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.DispatchStepAsync(999, 10));
    }

    [Fact]
    public async Task DispatchStepAsync_ThrowsWhenWorkstationNotFound()
    {
        var step = CreateStep(1, 100);
        _stepRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(step);
        _workstationRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Workstation?)null);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.DispatchStepAsync(1, 999));
    }

    [Fact]
    public async Task UndispatchStepAsync_ClearsWorkstation()
    {
        var step = CreateStep(1, 100, 10);

        _stepRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(step);
        _stepRepo.Setup(r => r.UpdateAsync(It.IsAny<WorkOrderStep>())).Returns(Task.CompletedTask);

        await _service.UndispatchStepAsync(1);

        _stepRepo.Verify(r => r.UpdateAsync(It.Is<WorkOrderStep>(s =>
            s.WorkstationId == null)), Times.Once);
    }

    [Fact]
    public async Task UndispatchStepAsync_ThrowsWhenStepNotFound()
    {
        _stepRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((WorkOrderStep?)null);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.UndispatchStepAsync(999));
    }

    [Fact]
    public async Task GetTodayDispatchedOrdersByLineAsync_ReturnsOrdersWithSteps()
    {
        var lineId = 1L;
        var workstations = new List<Workstation>
        {
            CreateWorkstation(10, lineId),
            CreateWorkstation(20, lineId)
        };
        var steps = new List<WorkOrderStep>
        {
            CreateStep(1, 100, 10),
            CreateStep(2, 100, 20)
        };
        var workOrder = CreateWorkOrder(100, WorkOrderStatus.IN_PROGRESS);

        _workstationRepo.Setup(r => r.FindAsync(w => w.LineId == lineId)).ReturnsAsync(workstations);
        _stepRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<WorkOrderStep, bool>>>()))
            .ReturnsAsync(steps);
        _workOrderRepo.Setup(r => r.GetByIdAsync(100)).ReturnsAsync(workOrder);

        var result = (await _service.GetTodayDispatchedOrdersByLineAsync(lineId)).ToList();

        Assert.Single(result);
    }

    [Fact]
    public async Task GetTodayDispatchedOrdersByLineAsync_ReturnsEmptyWhenNoWorkstations()
    {
        var lineId = 1L;

        _workstationRepo.Setup(r => r.FindAsync(w => w.LineId == lineId))
            .ReturnsAsync(Enumerable.Empty<Workstation>());

        var result = await _service.GetTodayDispatchedOrdersByLineAsync(lineId);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAvailableWorkstationsAsync_FiltersByLineAndStatus()
    {
        var lineId = 1L;
        var workstations = new List<Workstation>
        {
            CreateWorkstation(10, lineId, true),
            CreateWorkstation(20, lineId, true),
            CreateWorkstation(30, lineId, false) // disabled
        };

        _workstationRepo.Setup(r => r.FindAsync(w => w.LineId == lineId && w.Status))
            .ReturnsAsync(workstations.Take(2));

        var result = (await _service.GetAvailableWorkstationsAsync(lineId)).ToList();

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetAvailableWorkstationsAsync_ReturnsEmptyForUnknownLine()
    {
        _workstationRepo.Setup(r => r.FindAsync(w => w.LineId == 999 && w.Status))
            .ReturnsAsync(Enumerable.Empty<Workstation>());

        var result = await _service.GetAvailableWorkstationsAsync(999);

        Assert.Empty(result);
    }
}