using MES.Application.Services;
using MES.Domain.Entities;
using MES.Domain.Enums;
using MES.Domain.Exceptions;
using MES.Domain.Repositories;
using MES.Tests;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MES.Tests.Application;

public class EquipmentServiceTests
{
    private readonly Mock<IRepository<Equipment>> _equipmentRepo;
    private readonly Mock<IRepository<WorkReport>> _workReportRepo;
    private readonly Mock<IRepository<WorkOrder>> _workOrderRepo;
    private readonly Mock<IRepository<MaintenancePlan>> _maintenancePlanRepo;
    private readonly EquipmentService _service;

    public EquipmentServiceTests()
    {
        _equipmentRepo = new Mock<IRepository<Equipment>>();
        _workReportRepo = new Mock<IRepository<WorkReport>>();
        _workOrderRepo = new Mock<IRepository<WorkOrder>>();
        _maintenancePlanRepo = new Mock<IRepository<MaintenancePlan>>();

        _service = new EquipmentService(
            _equipmentRepo.Object,
            _workReportRepo.Object,
            _workOrderRepo.Object,
            _maintenancePlanRepo.Object);
    }

    private Equipment CreateEquipment(long id, EquipmentStatus status = EquipmentStatus.RUNNING)
    {
        var eq = Equipment.Create(
            $"EQ-{id:D3}",
            $"设备 {id}",
            maintainCycle: 30,
            theoreticalCycleTime: 10,
            plannedRunTime: 8);
        TestEntityFactory.SetProperty(eq, "Id", id);
        eq.SetStatus(status);
        return eq;
    }

    [Fact]
    public async Task RecordMaintenanceAsync_UpdatesMaintenanceDates()
    {
        var eq = CreateEquipment(1);
        _equipmentRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(eq);
        _equipmentRepo.Setup(r => r.UpdateAsync(It.IsAny<Equipment>())).Returns(Task.CompletedTask);

        await _service.RecordMaintenanceAsync(1);

        Assert.NotNull(eq.LastMaintainDate);
        Assert.NotNull(eq.NextMaintainDate);
        Assert.Equal(EquipmentStatus.RUNNING, eq.Status);
    }

    [Fact]
    public async Task RecordMaintenanceAsync_ThrowsWhenEquipmentNotFound()
    {
        _equipmentRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Equipment?)null);

        await Assert.ThrowsAsync<DomainException>(() =>
            _service.RecordMaintenanceAsync(999));
    }

    [Fact]
    public async Task ReportFaultAsync_SetsStatusToBroken()
    {
        var eq = CreateEquipment(1, EquipmentStatus.RUNNING);
        _equipmentRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(eq);
        _equipmentRepo.Setup(r => r.UpdateAsync(It.IsAny<Equipment>())).Returns(Task.CompletedTask);

        await _service.ReportFaultAsync(1);

        Assert.Equal(EquipmentStatus.BROKEN, eq.Status);
    }

    [Fact]
    public async Task CalculateOeeAsync_ReturnsDefaultWhenNoReports()
    {
        var eq = CreateEquipment(1);
        _equipmentRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(eq);
        _workReportRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<WorkReport, bool>>>()))
            .ReturnsAsync(Enumerable.Empty<WorkReport>());

        var result = await _service.CalculateOeeAsync(1);

        Assert.Equal(0, result.OeeValue);
        Assert.Equal(0, result.Availability);
        Assert.Equal(0, result.Performance);
        Assert.Equal(0, result.Quality);
    }

    [Fact]
    public async Task CalculateOeeAsync_CalculatesCorrectly()
    {
        var eq = CreateEquipment(1);
        var reports = new List<WorkReport>
        {
            TestEntityFactory.CreateWorkReportDirect(id: 1, workOrderId: 100, goodQty: 100, scrapQty: 5, reworkQty: 2, durationMin: 480, reportTime: DateTime.UtcNow.AddDays(-5)),
            TestEntityFactory.CreateWorkReportDirect(id: 2, workOrderId: 100, goodQty: 90, scrapQty: 3, reworkQty: 1, durationMin: 450, reportTime: DateTime.UtcNow.AddDays(-3))
        };

        _equipmentRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(eq);
        _workReportRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<WorkReport, bool>>>()))
            .ReturnsAsync(reports);

        var result = await _service.CalculateOeeAsync(1);

        Assert.True(result.OeeValue > 0);
        Assert.Equal(190, result.GoodQty);
        Assert.Equal(11, result.BadQty);
        Assert.True(result.Quality > 0 && result.Quality <= 1);
    }

    [Fact]
    public async Task CreateMaintenancePlanAsync_CreatesNewPlan()
    {
        var eq = CreateEquipment(1);
        _equipmentRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(eq);
        _equipmentRepo.Setup(r => r.UpdateAsync(It.IsAny<Equipment>())).Returns(Task.CompletedTask);
        _maintenancePlanRepo.Setup(r => r.AddAsync(It.IsAny<MaintenancePlan>()))
            .ReturnsAsync((MaintenancePlan p) => p);

        var result = await _service.CreateMaintenancePlanAsync(1, "日常保养", 30, "每30天一次");

        Assert.NotNull(result);
        Assert.Equal(1, result.EquipmentId);
        Assert.Equal("日常保养", result.PlanName);
        Assert.Equal(30, result.CycleDays);
    }

    [Fact]
    public async Task CompleteMaintenanceAsync_UpdatesPlanAndEquipment()
    {
        var eq = CreateEquipment(1);
        var plan = new MaintenancePlan(1, "日常保养", 30);

        _maintenancePlanRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(plan);
        _maintenancePlanRepo.Setup(r => r.UpdateAsync(It.IsAny<MaintenancePlan>())).Returns(Task.CompletedTask);
        _equipmentRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(eq);
        _equipmentRepo.Setup(r => r.UpdateAsync(It.IsAny<Equipment>())).Returns(Task.CompletedTask);

        await _service.CompleteMaintenanceAsync(1);

        Assert.Equal(MaintenancePlanStatus.COMPLETED, plan.Status);
        Assert.NotNull(plan.LastCompletedDate);
    }

    [Fact]
    public async Task GetMaintenancePlansAsync_ReturnsPlansForEquipment()
    {
        var plans = new List<MaintenancePlan>
        {
            new(1, "Plan 1", 30),
            new(1, "Plan 2", 60)
        };

        _maintenancePlanRepo.Setup(r => r.FindAsync(p => p.EquipmentId == 1))
            .ReturnsAsync(plans);

        var result = await _service.GetMaintenancePlansAsync(1);

        Assert.Equal(2, result.Count);
    }
}
