using MES.Domain.Entities;
using MES.Domain.Enums;
using MES.Domain.Exceptions;
using MES.Domain.ValueObjects;
using Xunit;

namespace MES.Tests.Domain;

public class WorkOrderTests
{
    #region Create 工厂方法

    [Fact]
    public void Create_SetsStatusToPending()
    {
        var wo = WorkOrder.Create("WO-001", SourceType.MANUAL, materialId: 1, plannedQty: new Quantity(100), priority: Priority.NORMAL);

        Assert.Equal(WorkOrderStatus.PENDING, wo.Status);
        Assert.Equal("WO-001", wo.OrderNo);
        Assert.Equal(SourceType.MANUAL, wo.SourceType);
        Assert.Equal(1, wo.MaterialId);
        Assert.Equal(100, wo.PlannedQty.Value);
        Assert.Equal(Priority.NORMAL, wo.Priority);
    }

    [Fact]
    public void Create_ThrowsWhenOrderNoIsEmpty()
    {
        Assert.Throws<DomainException>(() =>
            WorkOrder.Create("", SourceType.MANUAL, materialId: 1, plannedQty: new Quantity(100)));
    }

    [Fact]
    public void Create_ThrowsWhenOrderNoIsWhitespace()
    {
        Assert.Throws<DomainException>(() =>
            WorkOrder.Create("   ", SourceType.MANUAL, materialId: 1, plannedQty: new Quantity(100)));
    }

    [Fact]
    public void Create_ThrowsWhenMaterialIdInvalid()
    {
        Assert.Throws<DomainException>(() =>
            WorkOrder.Create("WO-001", SourceType.MANUAL, materialId: 0, plannedQty: new Quantity(100)));
    }

    [Fact]
    public void Create_ThrowsWhenPlannedQtyZero()
    {
        Assert.Throws<DomainException>(() =>
            WorkOrder.Create("WO-001", SourceType.MANUAL, materialId: 1, plannedQty: new Quantity(0)));
    }

    [Fact]
    public void Create_InitializesCompletedAndScrapQtyToZero()
    {
        var wo = WorkOrder.Create("WO-001", SourceType.MANUAL, materialId: 1, plannedQty: new Quantity(100));

        Assert.Equal(0, wo.CompletedQty.Value);
        Assert.Equal(0, wo.ScrapQty.Value);
    }

    #endregion

    #region Release 下达

    [Fact]
    public void Release_ChangesStatusToReleased_WhenPending()
    {
        var wo = CreateValidWorkOrder();
        wo.Release();

        Assert.Equal(WorkOrderStatus.RELEASED, wo.Status);
    }

    [Fact]
    public void Release_Throws_WhenNotPending()
    {
        var wo = CreateValidWorkOrder();
        wo.Release();

        Assert.Throws<DomainException>(() => wo.Release());
    }

    #endregion

    #region Schedule 排产

    [Fact]
    public void Schedule_ChangesStatusToScheduled_WhenReleased()
    {
        var wo = CreateValidWorkOrder();
        wo.Release();
        wo.Schedule(lineId: 1);

        Assert.Equal(WorkOrderStatus.SCHEDULED, wo.Status);
        Assert.Equal(1, wo.LineId);
    }

    [Fact]
    public void Schedule_Throws_WhenNotReleased()
    {
        var wo = CreateValidWorkOrder();

        Assert.Throws<DomainException>(() => wo.Schedule(lineId: 1));
    }

    [Fact]
    public void Schedule_Throws_WhenLineIdInvalid()
    {
        var wo = CreateValidWorkOrder();
        wo.Release();

        Assert.Throws<DomainException>(() => wo.Schedule(lineId: 0));
    }

    #endregion

    #region Unschedule 取消排产

    [Fact]
    public void Unschedule_ChangesStatusToReleased_WhenScheduled()
    {
        var wo = CreateValidWorkOrder();
        wo.Release();
        wo.Schedule(lineId: 1);
        wo.Unschedule();

        Assert.Equal(WorkOrderStatus.RELEASED, wo.Status);
        Assert.Null(wo.LineId);
    }

    [Fact]
    public void Unschedule_Throws_WhenNotScheduled()
    {
        var wo = CreateValidWorkOrder();
        wo.Release();

        Assert.Throws<DomainException>(() => wo.Unschedule());
    }

    #endregion

    #region Start 开始生产

    [Fact]
    public void Start_ChangesStatusToInProgress_WhenReleased()
    {
        var wo = CreateValidWorkOrder();
        wo.Release();
        wo.Start();

        Assert.Equal(WorkOrderStatus.IN_PROGRESS, wo.Status);
        Assert.NotNull(wo.ActualStartTime);
    }

    [Fact]
    public void Start_ChangesStatusToInProgress_WhenScheduled()
    {
        var wo = CreateValidWorkOrder();
        wo.Release();
        wo.Schedule(lineId: 1);
        wo.Start();

        Assert.Equal(WorkOrderStatus.IN_PROGRESS, wo.Status);
        Assert.NotNull(wo.ActualStartTime);
    }

    [Fact]
    public void Start_Throws_WhenPending()
    {
        var wo = CreateValidWorkOrder();

        Assert.Throws<DomainException>(() => wo.Start());
    }

    #endregion

    #region Hold 暂停

    [Fact]
    public void Hold_ChangesStatusToOnHold_FromReleased()
    {
        var wo = CreateValidWorkOrder();
        wo.Release();
        wo.Hold();

        Assert.Equal(WorkOrderStatus.ON_HOLD, wo.Status);
    }

    [Fact]
    public void Hold_ChangesStatusToOnHold_FromInProgress()
    {
        var wo = CreateValidWorkOrder();
        wo.Release();
        wo.Start();
        wo.Hold();

        Assert.Equal(WorkOrderStatus.ON_HOLD, wo.Status);
    }

    [Fact]
    public void Hold_Throws_WhenPending()
    {
        var wo = CreateValidWorkOrder();

        Assert.Throws<DomainException>(() => wo.Hold());
    }

    #endregion

    #region Resume 恢复

    [Fact]
    public void Resume_ChangesStatusToInProgress_FromOnHold()
    {
        var wo = CreateValidWorkOrder();
        wo.Release();
        wo.Start();
        wo.Hold();
        wo.Resume();

        Assert.Equal(WorkOrderStatus.IN_PROGRESS, wo.Status);
    }

    [Fact]
    public void Resume_Throws_WhenNotOnHold()
    {
        var wo = CreateValidWorkOrder();
        wo.Release();

        Assert.Throws<DomainException>(() => wo.Resume());
    }

    #endregion

    #region Complete 完成

    [Fact]
    public void Complete_ChangesStatusToCompleted_WhenInProgress()
    {
        var wo = CreateValidWorkOrder();
        wo.Release();
        wo.Start();
        wo.Complete();

        Assert.Equal(WorkOrderStatus.COMPLETED, wo.Status);
    }

    [Fact]
    public void Complete_Throws_WhenNotInProgress()
    {
        var wo = CreateValidWorkOrder();
        wo.Release();

        Assert.Throws<DomainException>(() => wo.Complete());
    }

    #endregion

    #region Close 关闭

    [Fact]
    public void Close_ChangesStatusToClosed_WhenCompleted()
    {
        var wo = CreateValidWorkOrder();
        wo.Release();
        wo.Start();
        wo.Complete();
        wo.Close();

        Assert.Equal(WorkOrderStatus.CLOSED, wo.Status);
        Assert.NotNull(wo.ActualEndTime);
    }

    [Fact]
    public void Close_Throws_WhenNotCompleted()
    {
        var wo = CreateValidWorkOrder();
        wo.Release();
        wo.Start();

        Assert.Throws<DomainException>(() => wo.Close());
    }

    #endregion

    #region Cancel 取消

    [Fact]
    public void Cancel_ChangesStatusToCancelled_WhenPending()
    {
        var wo = CreateValidWorkOrder();
        wo.Cancel();

        Assert.Equal(WorkOrderStatus.CANCELLED, wo.Status);
    }

    [Fact]
    public void Cancel_ChangesStatusToCancelled_WhenReleased()
    {
        var wo = CreateValidWorkOrder();
        wo.Release();
        wo.Cancel();

        Assert.Equal(WorkOrderStatus.CANCELLED, wo.Status);
    }

    [Fact]
    public void Cancel_ChangesStatusToCancelled_WhenOnHold()
    {
        var wo = CreateValidWorkOrder();
        wo.Release();
        wo.Hold();
        wo.Cancel();

        Assert.Equal(WorkOrderStatus.CANCELLED, wo.Status);
    }

    [Fact]
    public void Cancel_Throws_WhenInProgress()
    {
        var wo = CreateValidWorkOrder();
        wo.Release();
        wo.Start();

        Assert.Throws<DomainException>(() => wo.Cancel());
    }

    #endregion

    #region ReportProgress 报工

    [Fact]
    public void ReportProgress_AutoTransitionsToInProgress_WhenReleased()
    {
        var wo = CreateValidWorkOrder();
        wo.Release();
        wo.ReportProgress(new Quantity(10), new Quantity(0), new Quantity(0));

        Assert.Equal(WorkOrderStatus.IN_PROGRESS, wo.Status);
        Assert.Equal(10, wo.CompletedQty.Value);
        Assert.NotNull(wo.ActualStartTime);
    }

    [Fact]
    public void ReportProgress_AddsGoodAndReworkToCompletedQty()
    {
        var wo = CreateValidWorkOrder();
        wo.Release();
        wo.Start();
        wo.ReportProgress(new Quantity(30), new Quantity(5), new Quantity(3));

        Assert.Equal(33, wo.CompletedQty.Value);
        Assert.Equal(5, wo.ScrapQty.Value);
    }

    [Fact]
    public void ReportProgress_AutoCompletes_WhenQtyReachesPlanned()
    {
        var wo = CreateValidWorkOrder();
        wo.Release();
        wo.Start();
        wo.ReportProgress(new Quantity(100), new Quantity(0), new Quantity(0));

        Assert.Equal(WorkOrderStatus.COMPLETED, wo.Status);
    }

    [Fact]
    public void ReportProgress_Throws_WhenNegativeQty()
    {
        var wo = CreateValidWorkOrder();
        wo.Release();
        wo.Start();

        Assert.Throws<DomainException>(() =>
            wo.ReportProgress(new Quantity(-1), new Quantity(0), new Quantity(0)));
    }

    [Fact]
    public void ReportProgress_Throws_WhenInvalidStatus()
    {
        var wo = CreateValidWorkOrder();

        Assert.Throws<DomainException>(() =>
            wo.ReportProgress(new Quantity(10), new Quantity(0), new Quantity(0)));
    }

    #endregion

    #region Split 拆分

    [Fact]
    public void Split_ReducesPlannedQtyAndReturnsChild()
    {
        var wo = CreateValidWorkOrder();
        var child = wo.Split(new Quantity(30));

        Assert.Equal(70, wo.PlannedQty.Value);
        Assert.Equal(30, child.PlannedQty.Value);
        Assert.Equal("WO-TEST-SUB", child.OrderNo);
        Assert.Equal(WorkOrderStatus.PENDING, child.Status);
    }

    [Fact]
    public void Split_Throws_WhenInvalidStatus()
    {
        var wo = CreateValidWorkOrder();
        wo.Release();
        wo.Start();

        Assert.Throws<DomainException>(() => wo.Split(new Quantity(30)));
    }

    [Fact]
    public void Split_Throws_WhenSplitQtyExceedsPlanned()
    {
        var wo = CreateValidWorkOrder();

        Assert.Throws<DomainException>(() => wo.Split(new Quantity(100)));
    }

    #endregion

    #region Rework 返工

    [Fact]
    public void Rework_ReducesCompletedQtyAndReturnsChild()
    {
        var wo = CreateValidWorkOrder();
        wo.Release();
        wo.Start();
        wo.ReportProgress(new Quantity(50), new Quantity(0), new Quantity(0));
        wo.Complete();

        var child = wo.Rework(new Quantity(20));

        Assert.Equal(30, wo.CompletedQty.Value);
        Assert.Equal(20, child.PlannedQty.Value);
        Assert.Equal("WO-TEST-RWK", child.OrderNo);
        Assert.Equal(WorkOrderStatus.PENDING, child.Status);
        Assert.Equal(wo.Id, child.ReworkFromId);
    }

    [Fact]
    public void Rework_Throws_WhenInvalidStatus()
    {
        var wo = CreateValidWorkOrder();

        Assert.Throws<DomainException>(() => wo.Rework(new Quantity(10)));
    }

    [Fact]
    public void Rework_Throws_WhenReworkQtyExceedsCompleted()
    {
        var wo = CreateValidWorkOrder();
        wo.Release();
        wo.Start();
        wo.ReportProgress(new Quantity(10), new Quantity(0), new Quantity(0));
        wo.Complete();

        Assert.Throws<DomainException>(() => wo.Rework(new Quantity(20)));
    }

    #endregion

    #region Scrap 报废

    [Fact]
    public void Scrap_IncreasesScrapQty()
    {
        var wo = CreateValidWorkOrder();
        wo.Release();
        wo.Start();

        wo.Scrap(new Quantity(10), "defective");

        Assert.Equal(10, wo.ScrapQty.Value);
        Assert.Equal("defective", wo.Remark);
    }

    [Fact]
    public void Scrap_CancelsWhenFullyScrapped()
    {
        var wo = CreateValidWorkOrder();
        wo.Release();
        wo.Start();
        wo.ReportProgress(new Quantity(30), new Quantity(0), new Quantity(0));

        wo.Scrap(new Quantity(70), null);

        Assert.Equal(WorkOrderStatus.CANCELLED, wo.Status);
    }

    [Fact]
    public void Scrap_Throws_WhenExceedsRemaining()
    {
        var wo = CreateValidWorkOrder();
        wo.Release();
        wo.Start();
        wo.ReportProgress(new Quantity(80), new Quantity(0), new Quantity(0));

        Assert.Throws<DomainException>(() => wo.Scrap(new Quantity(30), null));
    }

    [Fact]
    public void Scrap_Throws_WhenInvalidStatus()
    {
        var wo = CreateValidWorkOrder();

        Assert.Throws<DomainException>(() => wo.Scrap(new Quantity(10), null));
    }

    #endregion

    #region Helper

    private static WorkOrder CreateValidWorkOrder()
    {
        return WorkOrder.Create("WO-TEST", SourceType.MANUAL, materialId: 1, plannedQty: new Quantity(100), priority: Priority.NORMAL);
    }

    #endregion
}
