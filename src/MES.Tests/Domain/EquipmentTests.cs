using MES.Domain.Entities;
using MES.Domain.Enums;
using MES.Domain.Exceptions;
using MES.Domain.ValueObjects;
using Xunit;

namespace MES.Tests.Domain;

public class EquipmentTests
{
    #region Create 工厂方法

    [Fact]
    public void Create_SetsStatusToIdle()
    {
        var eq = Equipment.Create("EQ-001", "Test Equipment");

        Assert.Equal(EquipmentStatus.IDLE, eq.Status);
        Assert.Equal("EQ-001", eq.Code);
        Assert.Equal("Test Equipment", eq.Name);
    }

    [Fact]
    public void Create_ThrowsWhenCodeIsEmpty()
    {
        Assert.Throws<DomainException>(() =>
            Equipment.Create("", "Test Equipment"));
    }

    [Fact]
    public void Create_ThrowsWhenNameIsEmpty()
    {
        Assert.Throws<DomainException>(() =>
            Equipment.Create("EQ-001", ""));
    }

    [Fact]
    public void Create_SetsOptionalFields()
    {
        var eq = Equipment.Create("EQ-001", "Test Equipment",
            model: "Model-X",
            factoryId: 1,
            workshopId: 2,
            lineId: 3,
            installDate: new DateTime(2024, 1, 1),
            maintainCycle: 30,
            theoreticalCycleTime: 60.0,
            plannedRunTime: 8.0);

        Assert.Equal("Model-X", eq.Model);
        Assert.Equal(1, eq.FactoryId);
        Assert.Equal(2, eq.WorkshopId);
        Assert.Equal(3, eq.LineId);
        Assert.Equal(new DateTime(2024, 1, 1), eq.InstallDate);
        Assert.Equal(30, eq.MaintainCycle);
        Assert.Equal(60.0, eq.TheoreticalCycleTime);
        Assert.Equal(8.0, eq.PlannedRunTime);
    }

    #endregion

    #region SetStatus 设置状态

    [Fact]
    public void SetStatus_ChangesStatus()
    {
        var eq = Equipment.Create("EQ-001", "Test Equipment");

        eq.SetStatus(EquipmentStatus.RUNNING);

        Assert.Equal(EquipmentStatus.RUNNING, eq.Status);
    }

    #endregion

    #region RecordMaintenance 记录保养

    [Fact]
    public void RecordMaintenance_SetsLastMaintainDateAndStatusToRunning()
    {
        var eq = Equipment.Create("EQ-001", "Test Equipment", maintainCycle: 30);
        eq.SetStatus(EquipmentStatus.MAINTENANCE);

        eq.RecordMaintenance();

        Assert.Equal(EquipmentStatus.RUNNING, eq.Status);
        Assert.NotNull(eq.LastMaintainDate);
        Assert.NotNull(eq.NextMaintainDate);
    }

    [Fact]
    public void RecordMaintenance_SetsNextMaintainDate_BasedOnCycle()
    {
        var eq = Equipment.Create("EQ-001", "Test Equipment", maintainCycle: 30);
        eq.SetStatus(EquipmentStatus.MAINTENANCE);

        eq.RecordMaintenance();

        Assert.NotNull(eq.NextMaintainDate);
        Assert.True(eq.NextMaintainDate!.Value.Date <= DateTime.UtcNow.AddDays(30).Date);
    }

    [Fact]
    public void RecordMaintenance_DoesNotSetNextMaintainDate_WhenNoCycle()
    {
        var eq = Equipment.Create("EQ-001", "Test Equipment");

        eq.RecordMaintenance();

        Assert.Null(eq.NextMaintainDate);
    }

    #endregion

    #region ReportFault 报修

    [Fact]
    public void ReportFault_SetsStatusToBroken()
    {
        var eq = Equipment.Create("EQ-001", "Test Equipment");
        eq.SetStatus(EquipmentStatus.RUNNING);

        eq.ReportFault();

        Assert.Equal(EquipmentStatus.BROKEN, eq.Status);
    }

    #endregion

    #region AddMaintenancePlan 添加保养计划

    [Fact]
    public void AddMaintenancePlan_AddsPlanToCollection()
    {
        var eq = Equipment.Create("EQ-001", "Test Equipment");
        TestEntityFactory.SetProperty(eq, "Id", 1L);

        var plan = eq.AddMaintenancePlan("Monthly Check", 30, "Regular maintenance");

        Assert.Single(eq.MaintenancePlans);
        Assert.Equal("Monthly Check", plan.PlanName);
        Assert.Equal(30, plan.CycleDays);
        Assert.Equal("Regular maintenance", plan.Description);
    }

    [Fact]
    public void AddMaintenancePlan_ThrowsWhenPlanNameEmpty()
    {
        var eq = Equipment.Create("EQ-001", "Test Equipment");
        TestEntityFactory.SetProperty(eq, "Id", 1L);

        Assert.Throws<DomainException>(() =>
            eq.AddMaintenancePlan("", 30));
    }

    [Fact]
    public void AddMaintenancePlan_ThrowsWhenCycleDaysZero()
    {
        var eq = Equipment.Create("EQ-001", "Test Equipment");
        TestEntityFactory.SetProperty(eq, "Id", 1L);

        Assert.Throws<DomainException>(() =>
            eq.AddMaintenancePlan("Plan", 0));
    }

    #endregion

    #region SetMaintainCycle 设置保养周期

    [Fact]
    public void SetMaintainCycle_SetsCycle()
    {
        var eq = Equipment.Create("EQ-001", "Test Equipment");

        eq.SetMaintainCycle(60);

        Assert.Equal(60, eq.MaintainCycle);
    }

    [Fact]
    public void SetMaintainCycle_ThrowsWhenNegative()
    {
        var eq = Equipment.Create("EQ-001", "Test Equipment");

        Assert.Throws<DomainException>(() => eq.SetMaintainCycle(-1));
    }

    #endregion

    #region UpdateTheoreticalCycleTime 更新理论节拍

    [Fact]
    public void UpdateTheoreticalCycleTime_SetsValue()
    {
        var eq = Equipment.Create("EQ-001", "Test Equipment");

        eq.UpdateTheoreticalCycleTime(45.5);

        Assert.Equal(45.5, eq.TheoreticalCycleTime);
    }

    [Fact]
    public void UpdateTheoreticalCycleTime_SetsNull()
    {
        var eq = Equipment.Create("EQ-001", "Test Equipment", theoreticalCycleTime: 60.0);

        eq.UpdateTheoreticalCycleTime(null);

        Assert.Null(eq.TheoreticalCycleTime);
    }

    [Fact]
    public void UpdateTheoreticalCycleTime_ThrowsWhenNegative()
    {
        var eq = Equipment.Create("EQ-001", "Test Equipment");

        Assert.Throws<DomainException>(() => eq.UpdateTheoreticalCycleTime(-1.0));
    }

    #endregion

    #region UpdatePlannedRunTime 更新日计划运行时间

    [Fact]
    public void UpdatePlannedRunTime_SetsValue()
    {
        var eq = Equipment.Create("EQ-001", "Test Equipment");

        eq.UpdatePlannedRunTime(7.5);

        Assert.Equal(7.5, eq.PlannedRunTime);
    }

    [Fact]
    public void UpdatePlannedRunTime_ThrowsWhenNegative()
    {
        var eq = Equipment.Create("EQ-001", "Test Equipment");

        Assert.Throws<DomainException>(() => eq.UpdatePlannedRunTime(-1.0));
    }

    #endregion

    #region Update 更新设备信息

    [Fact]
    public void Update_ChangesNameAndModel()
    {
        var eq = Equipment.Create("EQ-001", "Old Name", model: "Old Model");

        eq.Update("New Name", model: "New Model");

        Assert.Equal("New Name", eq.Name);
        Assert.Equal("New Model", eq.Model);
    }

    [Fact]
    public void Update_ThrowsWhenNameEmpty()
    {
        var eq = Equipment.Create("EQ-001", "Test Equipment");

        Assert.Throws<DomainException>(() => eq.Update(""));
    }

    #endregion

    #region OEE 计算静态方法

    [Fact]
    public void CalculateOee_ReturnsCorrectResult()
    {
        var result = Equipment.CalculateOee(0.9m, 0.8m, 0.95m);

        Assert.Equal(0.9m, result.Availability);
        Assert.Equal(0.8m, result.Performance);
        Assert.Equal(0.95m, result.Quality);
        Assert.Equal(0.9m * 0.8m * 0.95m, result.Oee);
    }

    [Fact]
    public void CalculateOee_ThrowsWhenAvailabilityOutOfRange()
    {
        Assert.Throws<DomainException>(() =>
            Equipment.CalculateOee(1.5m, 0.8m, 0.95m));
    }

    [Fact]
    public void CalculateOee_ThrowsWhenPerformanceOutOfRange()
    {
        Assert.Throws<DomainException>(() =>
            Equipment.CalculateOee(0.9m, -0.1m, 0.95m));
    }

    [Fact]
    public void CalculateOee_ThrowsWhenQualityOutOfRange()
    {
        Assert.Throws<DomainException>(() =>
            Equipment.CalculateOee(0.9m, 0.8m, 2.0m));
    }

    [Fact]
    public void CalculateAvailability_ReturnsCorrectValue()
    {
        var result = Equipment.CalculateAvailability(8m, 10m);

        Assert.Equal(0.8m, result);
    }

    [Fact]
    public void CalculateAvailability_CapsAtOne()
    {
        var result = Equipment.CalculateAvailability(12m, 10m);

        Assert.Equal(1m, result);
    }

    [Fact]
    public void CalculateAvailability_ThrowsWhenPlannedRunTimeZero()
    {
        Assert.Throws<DomainException>(() =>
            Equipment.CalculateAvailability(8m, 0m));
    }

    [Fact]
    public void CalculateAvailability_ThrowsWhenActualRunTimeNegative()
    {
        Assert.Throws<DomainException>(() =>
            Equipment.CalculateAvailability(-1m, 10m));
    }

    [Fact]
    public void CalculatePerformance_ReturnsCorrectValue()
    {
        var result = Equipment.CalculatePerformance(60m, 100m, 7200m);

        Assert.Equal(60m * 100m / 7200m, result);
    }

    [Fact]
    public void CalculatePerformance_CapsAtOne()
    {
        var result = Equipment.CalculatePerformance(60m, 200m, 7200m);

        Assert.Equal(1m, result);
    }

    [Fact]
    public void CalculatePerformance_ThrowsWhenCycleTimeZero()
    {
        Assert.Throws<DomainException>(() =>
            Equipment.CalculatePerformance(0m, 100m, 7200m));
    }

    [Fact]
    public void CalculatePerformance_ThrowsWhenActualRunTimeZero()
    {
        Assert.Throws<DomainException>(() =>
            Equipment.CalculatePerformance(60m, 100m, 0m));
    }

    [Fact]
    public void CalculateQuality_ReturnsCorrectValue()
    {
        var result = Equipment.CalculateQuality(95m, 100m);

        Assert.Equal(0.95m, result);
    }

    [Fact]
    public void CalculateQuality_ThrowsWhenTotalCountZero()
    {
        Assert.Throws<DomainException>(() =>
            Equipment.CalculateQuality(95m, 0m));
    }

    [Fact]
    public void CalculateQuality_ThrowsWhenGoodCountExceedsTotal()
    {
        Assert.Throws<DomainException>(() =>
            Equipment.CalculateQuality(110m, 100m));
    }

    [Fact]
    public void CalculateQuality_ThrowsWhenGoodCountNegative()
    {
        Assert.Throws<DomainException>(() =>
            Equipment.CalculateQuality(-1m, 100m));
    }

    #endregion
}
