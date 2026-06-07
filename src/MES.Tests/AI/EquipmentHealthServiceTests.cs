using MES.AI.Application.Dtos;
using MES.AI.Application.Services;
using MES.Domain.Entities;
using MES.Domain.Enums;
using MES.Infrastructure.Repositories;
using Moq;
using Xunit;

namespace MES.Tests.AI;

public class EquipmentHealthServiceTests
{
    private readonly Mock<IRepository<Equipment>> _equipmentRepo;
    private readonly Mock<IRepository<WorkReport>> _workReportRepo;
    private readonly Mock<IRepository<WorkOrder>> _workOrderRepo;
    private readonly EquipmentHealthService _service;

    public EquipmentHealthServiceTests()
    {
        _equipmentRepo = new Mock<IRepository<Equipment>>();
        _workReportRepo = new Mock<IRepository<WorkReport>>();
        _workOrderRepo = new Mock<IRepository<WorkOrder>>();

        _service = new EquipmentHealthService(
            _equipmentRepo.Object,
            _workReportRepo.Object,
            _workOrderRepo.Object);
    }

    [Fact]
    public async Task AnalyzeEquipmentAsync_Throws_WhenEquipmentNotFound()
    {
        _equipmentRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Equipment?)null);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.AnalyzeEquipmentAsync(999));
    }

    [Fact]
    public async Task AnalyzeEquipmentAsync_ReturnsResult_WhenValidEquipment()
    {
        var equipment = new Equipment
        {
            Id = 1,
            Name = "TestEquipment",
            Status = EquipmentStatus.RUNNING,
            PlannedRunTime = 480,
            TheoreticalCycleTime = 30
        };

        _equipmentRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(equipment);
        _workReportRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<WorkReport, bool>>>()))
            .ReturnsAsync(new List<WorkReport>().AsEnumerable());

        var result = await _service.AnalyzeEquipmentAsync(1);

        Assert.NotNull(result);
        Assert.Equal(1, result.EquipmentId);
        Assert.Equal("TestEquipment", result.EquipmentName);
    }

    [Fact]
    public async Task GetAllEquipmentHealthAsync_ReturnsList()
    {
        var equipment = new List<Equipment>
        {
            new() { Id = 1, Name = "Equipment1", Status = EquipmentStatus.RUNNING, PlannedRunTime = 480 },
            new() { Id = 2, Name = "Equipment2", Status = EquipmentStatus.RUNNING, PlannedRunTime = 480 }
        };

        _equipmentRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(equipment);
        _workReportRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<WorkReport, bool>>>()))
            .ReturnsAsync(new List<WorkReport>().AsEnumerable());

        var result = await _service.GetAllEquipmentHealthAsync();

        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetHighRiskEquipmentAsync_ReturnsEmpty_WhenAllHealthy()
    {
        var equipment = new List<Equipment>
        {
            new() { Id = 1, Name = "Equipment1", Status = EquipmentStatus.RUNNING, PlannedRunTime = 480 }
        };

        _equipmentRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(equipment);
        _workReportRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<WorkReport, bool>>>()))
            .ReturnsAsync(new List<WorkReport>().AsEnumerable());

        var result = await _service.GetHighRiskEquipmentAsync();

        Assert.NotNull(result);
    }
}