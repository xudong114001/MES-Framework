using MES.Domain.Enums;
using Xunit;

namespace MES.Tests.Domain;

public class EnumsTests
{
    [Fact]
    public void Priority_HasExplicitValues()
    {
        Assert.Equal(0, (int)Priority.LOW);
        Assert.Equal(50, (int)Priority.NORMAL);
        Assert.Equal(80, (int)Priority.HIGH);
        Assert.Equal(100, (int)Priority.URGENT);
    }

    [Fact]
    public void Priority_DefaultIsNormal()
    {
        var defaultPriority = default(Priority);
        Assert.Equal(Priority.LOW, defaultPriority);
    }

    [Fact]
    public void Priority_OrderingByValue()
    {
        Assert.True((int)Priority.LOW < (int)Priority.NORMAL);
        Assert.True((int)Priority.NORMAL < (int)Priority.HIGH);
        Assert.True((int)Priority.HIGH < (int)Priority.URGENT);
    }

    [Fact]
    public void Priority_ParseFromInt()
    {
        Assert.Equal(Priority.LOW, (Priority)0);
        Assert.Equal(Priority.NORMAL, (Priority)50);
        Assert.Equal(Priority.HIGH, (Priority)80);
        Assert.Equal(Priority.URGENT, (Priority)100);
    }

    [Fact]
    public void SourceType_HasTwoValues()
    {
        var values = Enum.GetValues<SourceType>();
        Assert.Equal(2, values.Length);
        Assert.Contains(SourceType.ERP, values);
        Assert.Contains(SourceType.MANUAL, values);
    }

    [Fact]
    public void ReportType_HasThreeValues()
    {
        var values = Enum.GetValues<ReportType>();
        Assert.Equal(3, values.Length);
        Assert.Contains(ReportType.COMPLETE, values);
        Assert.Contains(ReportType.REWORK, values);
        Assert.Contains(ReportType.SCRAP, values);
    }

    [Fact]
    public void QcResult_HasThreeValues()
    {
        var values = Enum.GetValues<QcResult>();
        Assert.Equal(3, values.Length);
        Assert.Contains(QcResult.PASS, values);
        Assert.Contains(QcResult.FAIL, values);
        Assert.Contains(QcResult.PENDING, values);
    }

    [Fact]
    public void QcInspectionType_HasFourValues()
    {
        var values = Enum.GetValues<QcInspectionType>();
        Assert.Equal(4, values.Length);
        Assert.Contains(QcInspectionType.INCOMING, values);
        Assert.Contains(QcInspectionType.FIRST, values);
        Assert.Contains(QcInspectionType.PATROL, values);
        Assert.Contains(QcInspectionType.FINAL, values);
    }

    [Fact]
    public void InspectionResult_HasFourValues()
    {
        var values = Enum.GetValues<InspectionResult>();
        Assert.Equal(4, values.Length);
        Assert.Contains(InspectionResult.ACCEPT, values);
        Assert.Contains(InspectionResult.REWORK, values);
        Assert.Contains(InspectionResult.SCRAP, values);
        Assert.Contains(InspectionResult.CONCESSION, values);
    }

    [Fact]
    public void EquipmentStatus_HasFourValues()
    {
        var values = Enum.GetValues<EquipmentStatus>();
        Assert.Equal(4, values.Length);
        Assert.Contains(EquipmentStatus.RUNNING, values);
        Assert.Contains(EquipmentStatus.IDLE, values);
        Assert.Contains(EquipmentStatus.MAINTENANCE, values);
        Assert.Contains(EquipmentStatus.BROKEN, values);
    }

    [Fact]
    public void MaintenancePlanStatus_HasThreeValues()
    {
        var values = Enum.GetValues<MaintenancePlanStatus>();
        Assert.Equal(3, values.Length);
        Assert.Contains(MaintenancePlanStatus.PENDING, values);
        Assert.Contains(MaintenancePlanStatus.COMPLETED, values);
        Assert.Contains(MaintenancePlanStatus.OVERDUE, values);
    }

    [Fact]
    public void LineType_HasThreeValues()
    {
        var values = Enum.GetValues<LineType>();
        Assert.Equal(3, values.Length);
        Assert.Contains(LineType.FLOW, values);
        Assert.Contains(LineType.CELL, values);
        Assert.Contains(LineType.FIXED, values);
    }

    [Theory]
    [InlineData(typeof(WorkOrderStatus), 8)]
    [InlineData(typeof(SourceType), 2)]
    [InlineData(typeof(ReportType), 3)]
    [InlineData(typeof(QcResult), 3)]
    [InlineData(typeof(QcInspectionType), 4)]
    [InlineData(typeof(InspectionResult), 4)]
    [InlineData(typeof(EquipmentStatus), 4)]
    [InlineData(typeof(MaintenancePlanStatus), 3)]
    [InlineData(typeof(LineType), 3)]
    [InlineData(typeof(Priority), 4)]
    public void AllEnums_HaveExpectedMemberCount(Type enumType, int expectedCount)
    {
        var count = Enum.GetValues(enumType).Length;
        Assert.Equal(expectedCount, count);
    }

    [Fact]
    public void AllEnums_AreInDomainEnumsNamespace()
    {
        var enumTypes = new[]
        {
            typeof(WorkOrderStatus), typeof(SourceType), typeof(ReportType),
            typeof(QcResult), typeof(QcInspectionType), typeof(InspectionResult),
            typeof(EquipmentStatus), typeof(MaintenancePlanStatus),
            typeof(LineType), typeof(Priority)
        };

        foreach (var type in enumTypes)
        {
            Assert.Equal("MES.Domain.Enums", type.Namespace);
        }
    }

    [Fact]
    public void Priority_CastToInt_RoundTrip()
    {
        foreach (Priority p in Enum.GetValues<Priority>())
        {
            var asInt = (int)p;
            var roundTripped = (Priority)asInt;
            Assert.Equal(p, roundTripped);
        }
    }

    [Fact]
    public void WorkOrderStatus_AllValuesHaveNames()
    {
        foreach (WorkOrderStatus status in Enum.GetValues<WorkOrderStatus>())
        {
            var name = status.ToString();
            Assert.False(string.IsNullOrEmpty(name));
            Assert.Equal(name, Enum.GetName(status));
        }
    }
}
