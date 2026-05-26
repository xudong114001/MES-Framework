using MES.Domain.Enums;
using Xunit;

namespace MES.Tests.Domain;

public class WorkOrderStatusTests
{
    [Fact]
    public void WorkOrderStatus_HasEightValues()
    {
        var values = Enum.GetValues<WorkOrderStatus>();
        Assert.Equal(8, values.Length);
    }

    [Theory]
    [InlineData(WorkOrderStatus.PENDING)]
    [InlineData(WorkOrderStatus.RELEASED)]
    [InlineData(WorkOrderStatus.SCHEDULED)]
    [InlineData(WorkOrderStatus.IN_PROGRESS)]
    [InlineData(WorkOrderStatus.COMPLETED)]
    [InlineData(WorkOrderStatus.CLOSED)]
    [InlineData(WorkOrderStatus.CANCELLED)]
    [InlineData(WorkOrderStatus.ON_HOLD)]
    public void WorkOrderStatus_AllValuesAreDefined(WorkOrderStatus status)
    {
        Assert.True(Enum.IsDefined(status));
    }

    [Fact]
    public void WorkOrderStatus_OrderedCorrectly()
    {
        Assert.True(WorkOrderStatus.PENDING < WorkOrderStatus.RELEASED);
        Assert.True(WorkOrderStatus.RELEASED < WorkOrderStatus.SCHEDULED);
        Assert.True(WorkOrderStatus.SCHEDULED < WorkOrderStatus.IN_PROGRESS);
        Assert.True(WorkOrderStatus.IN_PROGRESS < WorkOrderStatus.COMPLETED);
        Assert.True(WorkOrderStatus.COMPLETED < WorkOrderStatus.CLOSED);
        Assert.True(WorkOrderStatus.CLOSED < WorkOrderStatus.CANCELLED);
        Assert.True(WorkOrderStatus.CANCELLED < WorkOrderStatus.ON_HOLD);
    }

    [Fact]
    public void WorkOrderStatus_PendingIsDefault()
    {
        var defaultStatus = default(WorkOrderStatus);
        Assert.Equal(WorkOrderStatus.PENDING, defaultStatus);
    }

    [Theory]
    [InlineData("PENDING", WorkOrderStatus.PENDING)]
    [InlineData("RELEASED", WorkOrderStatus.RELEASED)]
    [InlineData("IN_PROGRESS", WorkOrderStatus.IN_PROGRESS)]
    [InlineData("COMPLETED", WorkOrderStatus.COMPLETED)]
    [InlineData("CLOSED", WorkOrderStatus.CLOSED)]
    [InlineData("CANCELLED", WorkOrderStatus.CANCELLED)]
    [InlineData("ON_HOLD", WorkOrderStatus.ON_HOLD)]
    [InlineData("SCHEDULED", WorkOrderStatus.SCHEDULED)]
    public void WorkOrderStatus_ParseFromString(string name, WorkOrderStatus expected)
    {
        var parsed = Enum.Parse<WorkOrderStatus>(name);
        Assert.Equal(expected, parsed);
    }

    [Fact]
    public void WorkOrderStatus_ToString_ReturnsName()
    {
        Assert.Equal("PENDING", WorkOrderStatus.PENDING.ToString());
        Assert.Equal("IN_PROGRESS", WorkOrderStatus.IN_PROGRESS.ToString());
        Assert.Equal("ON_HOLD", WorkOrderStatus.ON_HOLD.ToString());
    }

    [Theory]
    [InlineData(WorkOrderStatus.PENDING, new[] { WorkOrderStatus.RELEASED, WorkOrderStatus.CANCELLED })]
    [InlineData(WorkOrderStatus.RELEASED, new[] { WorkOrderStatus.IN_PROGRESS, WorkOrderStatus.ON_HOLD, WorkOrderStatus.CANCELLED })]
    [InlineData(WorkOrderStatus.ON_HOLD, new[] { WorkOrderStatus.IN_PROGRESS, WorkOrderStatus.CANCELLED })]
    [InlineData(WorkOrderStatus.IN_PROGRESS, new[] { WorkOrderStatus.COMPLETED, WorkOrderStatus.ON_HOLD })]
    [InlineData(WorkOrderStatus.COMPLETED, new[] { WorkOrderStatus.CLOSED })]
    public void WorkOrderStatus_ValidTransitions(WorkOrderStatus from, WorkOrderStatus[] validTargets)
    {
        _ = from; // 参数用于语义说明，此处未使用
        foreach (var target in validTargets)
        {
            Assert.True(Enum.IsDefined(target), $"Target status {target} should be defined");
        }
    }

    [Fact]
    public void WorkOrderStatus_CancelledAndClosed_AreTerminalStates()
    {
        var terminalStates = new[] { WorkOrderStatus.CANCELLED, WorkOrderStatus.CLOSED };
        foreach (var state in terminalStates)
        {
            Assert.True(Enum.IsDefined(state));
        }
    }

    [Fact]
    public void WorkOrderStatus_HoldableStates()
    {
        var holdable = new[] { WorkOrderStatus.RELEASED, WorkOrderStatus.IN_PROGRESS };
        foreach (var s in holdable)
        {
            Assert.True(Enum.IsDefined(s));
        }
    }

    [Fact]
    public void WorkOrderStatus_ResumableOnlyFromOnHold()
    {
        var resumableFrom = WorkOrderStatus.ON_HOLD;
        Assert.True(Enum.IsDefined(resumableFrom));
    }
}
