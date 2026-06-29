using MES.Application.Dtos;
using MES.Application.Services;
using MES.Domain.Entities;
using MES.Domain.Enums;
using MES.Domain.Repositories;
using Moq;
using Xunit;

namespace MES.Tests.Application;

public class DashboardServiceTests
{
    private readonly Mock<IRepository<WorkOrder>> _workOrderRepo;
    private readonly Mock<IRepository<Equipment>> _equipmentRepo;
    private readonly DashboardService _service;

    public DashboardServiceTests()
    {
        _workOrderRepo = new Mock<IRepository<WorkOrder>>();
        _equipmentRepo = new Mock<IRepository<Equipment>>();
        _service = new DashboardService(_workOrderRepo.Object, _equipmentRepo.Object);
    }

    #region GetTodayOrderStatsAsync

    [Fact]
    public async Task GetTodayOrderStatsAsync_ReturnsTodayOrderStatsDto()
    {
        var orders = new List<WorkOrder>
        {
            TestEntityFactory.CreateWorkOrderWithStatus("WO-001", 1, 100, WorkOrderStatus.PENDING),
            TestEntityFactory.CreateWorkOrderWithStatus("WO-002", 2, 100, WorkOrderStatus.IN_PROGRESS),
            TestEntityFactory.CreateWorkOrderWithStatus("WO-003", 3, 100, WorkOrderStatus.COMPLETED),
            TestEntityFactory.CreateWorkOrderWithStatus("WO-004", 4, 100, WorkOrderStatus.PENDING),
        };

        _workOrderRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<WorkOrder, bool>>>()))
            .ReturnsAsync(orders);

        var result = await _service.GetTodayOrderStatsAsync();

        Assert.NotNull(result);
        Assert.IsType<TodayOrderStatsDto>(result);
        Assert.Equal(4, result.Total);
        Assert.Equal(2, result.Pending);
        Assert.Equal(1, result.InProgress);
        Assert.Equal(1, result.Completed);
    }

    [Fact]
    public async Task GetTodayOrderStatsAsync_ReturnsEmptyStats_WhenNoOrders()
    {
        _workOrderRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<WorkOrder, bool>>>()))
            .ReturnsAsync([]);

        var result = await _service.GetTodayOrderStatsAsync();

        Assert.NotNull(result);
        Assert.Equal(0, result.Total);
        Assert.Equal(0, result.Pending);
        Assert.Equal(0, result.InProgress);
        Assert.Equal(0, result.Completed);
    }

    #endregion

    #region GetOrderStatusDistributionAsync

    [Fact]
    public async Task GetOrderStatusDistributionAsync_ReturnsDistributionDtos()
    {
        var orders = new List<WorkOrder>
        {
            TestEntityFactory.CreateWorkOrderWithStatus("WO-001", 1, 100, WorkOrderStatus.PENDING),
            TestEntityFactory.CreateWorkOrderWithStatus("WO-002", 2, 100, WorkOrderStatus.PENDING),
            TestEntityFactory.CreateWorkOrderWithStatus("WO-003", 3, 100, WorkOrderStatus.IN_PROGRESS),
        };

        _workOrderRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(orders);

        var result = await _service.GetOrderStatusDistributionAsync();

        Assert.NotNull(result);
        var list = result.ToList();
        Assert.IsType<List<OrderStatusDistributionDto>>(list);
        Assert.Equal(2, list.Count);
    }

    [Fact]
    public async Task GetOrderStatusDistributionAsync_GroupsByStatus()
    {
        var orders = new List<WorkOrder>
        {
            TestEntityFactory.CreateWorkOrderWithStatus("WO-001", 1, 100, WorkOrderStatus.PENDING),
            TestEntityFactory.CreateWorkOrderWithStatus("WO-002", 2, 100, WorkOrderStatus.PENDING),
            TestEntityFactory.CreateWorkOrderWithStatus("WO-003", 3, 100, WorkOrderStatus.COMPLETED),
        };

        _workOrderRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(orders);

        var result = await _service.GetOrderStatusDistributionAsync();
        var list = result.ToList();

        var pendingGroup = list.FirstOrDefault(d => d.Status == "PENDING");
        var completedGroup = list.FirstOrDefault(d => d.Status == "COMPLETED");

        Assert.NotNull(pendingGroup);
        Assert.Equal(2, pendingGroup.Count);
        Assert.NotNull(completedGroup);
        Assert.Equal(1, completedGroup.Count);
    }

    #endregion

    #region GetOutputStatsAsync

    [Fact]
    public async Task GetOutputStatsAsync_ReturnsOutputStatsDto()
    {
        var wo1 = TestEntityFactory.CreateWorkOrderWithStatus("WO-001", 1, 100, WorkOrderStatus.COMPLETED, completedQty: 80, scrapQty: 5);
        var wo2 = TestEntityFactory.CreateWorkOrderWithStatus("WO-002", 2, 100, WorkOrderStatus.COMPLETED, completedQty: 60, scrapQty: 3);

        _workOrderRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<WorkOrder, bool>>>()))
            .ReturnsAsync(new[] { wo1, wo2 });

        var result = await _service.GetOutputStatsAsync();

        Assert.NotNull(result);
        Assert.IsType<OutputStatsDto>(result);
        Assert.Equal(140, result.GoodQty);
        Assert.Equal(8, result.ScrapQty);
        Assert.Equal(2, result.WorkOrderCount);
    }

    #endregion

    #region GetEquipmentStatusAsync

    [Fact]
    public async Task GetEquipmentStatusAsync_ReturnsEquipmentStatusDtos()
    {
        var equipment = new List<Equipment>
        {
            TestEntityFactory.CreateEquipment("EQ-001", "Equipment 1", status: EquipmentStatus.RUNNING),
            TestEntityFactory.CreateEquipment("EQ-002", "Equipment 2", status: EquipmentStatus.RUNNING),
            TestEntityFactory.CreateEquipment("EQ-003", "Equipment 3", status: EquipmentStatus.IDLE),
        };

        _equipmentRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(equipment);

        var result = await _service.GetEquipmentStatusAsync();

        Assert.NotNull(result);
        var list = result.ToList();
        Assert.IsType<List<EquipmentStatusDto>>(list);
        Assert.Equal(2, list.Count);
    }

    [Fact]
    public async Task GetEquipmentStatusAsync_GroupsByStatus()
    {
        var equipment = new List<Equipment>
        {
            TestEntityFactory.CreateEquipment("EQ-001", "Equipment 1", status: EquipmentStatus.RUNNING),
            TestEntityFactory.CreateEquipment("EQ-002", "Equipment 2", status: EquipmentStatus.RUNNING),
            TestEntityFactory.CreateEquipment("EQ-003", "Equipment 3", status: EquipmentStatus.IDLE),
        };

        _equipmentRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(equipment);

        var result = await _service.GetEquipmentStatusAsync();
        var list = result.ToList();

        var runningGroup = list.FirstOrDefault(d => d.Status == "RUNNING");
        var idleGroup = list.FirstOrDefault(d => d.Status == "IDLE");

        Assert.NotNull(runningGroup);
        Assert.Equal(2, runningGroup.Count);
        Assert.NotNull(idleGroup);
        Assert.Equal(1, idleGroup.Count);
    }

    #endregion
}
