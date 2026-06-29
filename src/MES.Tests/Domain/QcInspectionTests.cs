using MES.Domain.Entities;
using MES.Domain.Enums;
using MES.Domain.Exceptions;
using Xunit;

namespace MES.Tests.Domain;

public class QcInspectionTests
{
    #region Create 工厂方法

    [Fact]
    public void Create_SetsInspectResultToPending()
    {
        var qc = QcInspection.Create("QC-001", QcInspectionType.FINAL);

        Assert.Equal(QcResult.PENDING, qc.InspectResult);
        Assert.Equal("QC-001", qc.InspectNo);
        Assert.Equal(QcInspectionType.FINAL, qc.SourceType);
    }

    [Fact]
    public void Create_ThrowsWhenInspectNoIsEmpty()
    {
        Assert.Throws<DomainException>(() =>
            QcInspection.Create("", QcInspectionType.FINAL));
    }

    [Fact]
    public void Create_ThrowsWhenInspectNoIsWhitespace()
    {
        Assert.Throws<DomainException>(() =>
            QcInspection.Create("   ", QcInspectionType.FINAL));
    }

    [Fact]
    public void Create_SetsOptionalFields()
    {
        var qc = QcInspection.Create("QC-001", QcInspectionType.FIRST,
            workOrderId: 10,
            materialId: 20,
            inspector: 5,
            sourceRef: "REF-001",
            remark: "Test remark");

        Assert.Equal(10, qc.WorkOrderId);
        Assert.Equal(20, qc.MaterialId);
        Assert.Equal(5, qc.Inspector);
        Assert.Equal("REF-001", qc.SourceRef);
        Assert.Equal("Test remark", qc.Remark);
    }

    #endregion

    #region Verify 判定

    [Fact]
    public void Verify_SetsResultToPass_WhenPending()
    {
        var qc = QcInspection.Create("QC-001", QcInspectionType.FINAL);

        qc.Verify(QcResult.PASS);

        Assert.Equal(QcResult.PASS, qc.InspectResult);
        Assert.NotEqual(default(DateTime), qc.InspectTime);
    }

    [Fact]
    public void Verify_SetsResultToFail_WhenPending()
    {
        var qc = QcInspection.Create("QC-001", QcInspectionType.FINAL);

        qc.Verify(QcResult.FAIL);

        Assert.Equal(QcResult.FAIL, qc.InspectResult);
    }

    [Fact]
    public void Verify_Throws_WhenAlreadyVerified()
    {
        var qc = QcInspection.Create("QC-001", QcInspectionType.FINAL);
        qc.Verify(QcResult.PASS);

        Assert.Throws<DomainException>(() => qc.Verify(QcResult.FAIL));
    }

    [Fact]
    public void Verify_Throws_WhenAlreadyFailed()
    {
        var qc = QcInspection.Create("QC-001", QcInspectionType.FINAL);
        qc.Verify(QcResult.FAIL);

        Assert.Throws<DomainException>(() => qc.Verify(QcResult.PASS));
    }

    #endregion

    #region HandleNonconforming 不合格品处理

    [Fact]
    public void HandleNonconforming_SetsHandlingAction_WhenFail()
    {
        var qc = QcInspection.Create("QC-001", QcInspectionType.FINAL);
        qc.Verify(QcResult.FAIL);

        qc.HandleNonconforming(InspectionResult.SCRAP, "报废处理");

        Assert.Equal(InspectionResult.SCRAP, qc.HandlingAction);
        Assert.Equal("报废处理", qc.HandlingRemark);
        Assert.NotNull(qc.HandledAt);
    }

    [Fact]
    public void HandleNonconforming_SetsReworkAction()
    {
        var qc = QcInspection.Create("QC-001", QcInspectionType.FINAL);
        qc.Verify(QcResult.FAIL);

        qc.HandleNonconforming(InspectionResult.REWORK, "返工处理");

        Assert.Equal(InspectionResult.REWORK, qc.HandlingAction);
    }

    [Fact]
    public void HandleNonconforming_SetsConcessionAction()
    {
        var qc = QcInspection.Create("QC-001", QcInspectionType.FINAL);
        qc.Verify(QcResult.FAIL);

        qc.HandleNonconforming(InspectionResult.CONCESSION, "让步接收");

        Assert.Equal(InspectionResult.CONCESSION, qc.HandlingAction);
    }

    [Fact]
    public void HandleNonconforming_Throws_WhenNotFailed()
    {
        var qc = QcInspection.Create("QC-001", QcInspectionType.FINAL);
        qc.Verify(QcResult.PASS);

        Assert.Throws<DomainException>(() =>
            qc.HandleNonconforming(InspectionResult.SCRAP, "test"));
    }

    [Fact]
    public void HandleNonconforming_Throws_WhenStillPending()
    {
        var qc = QcInspection.Create("QC-001", QcInspectionType.FINAL);

        Assert.Throws<DomainException>(() =>
            qc.HandleNonconforming(InspectionResult.SCRAP, "test"));
    }

    [Fact]
    public void HandleNonconforming_Throws_WhenInvalidAction()
    {
        var qc = QcInspection.Create("QC-001", QcInspectionType.FINAL);
        qc.Verify(QcResult.FAIL);

        Assert.Throws<DomainException>(() =>
            qc.HandleNonconforming(InspectionResult.ACCEPT, "test"));
    }

    #endregion

    #region AddItem / RemoveItem 质检项

    [Fact]
    public void AddItem_AddsItemToCollection()
    {
        var qc = QcInspection.Create("QC-001", QcInspectionType.FINAL);
        TestEntityFactory.SetProperty(qc, "Id", 1L);

        var item = CreateQcInspectionItem();

        qc.AddItem(item);

        Assert.Single(qc.Items);
    }

    [Fact]
    public void AddItem_Throws_WhenItemIsNull()
    {
        var qc = QcInspection.Create("QC-001", QcInspectionType.FINAL);

        Assert.Throws<DomainException>(() => qc.AddItem(null!));
    }

    [Fact]
    public void RemoveItem_RemovesItemFromCollection()
    {
        var qc = QcInspection.Create("QC-001", QcInspectionType.FINAL);
        TestEntityFactory.SetProperty(qc, "Id", 1L);

        var item = CreateQcInspectionItem();
        qc.AddItem(item);

        qc.RemoveItem(item);

        Assert.Empty(qc.Items);
    }

    [Fact]
    public void RemoveItem_Throws_WhenItemIsNull()
    {
        var qc = QcInspection.Create("QC-001", QcInspectionType.FINAL);

        Assert.Throws<DomainException>(() => qc.RemoveItem(null!));
    }

    #endregion

    #region Helper

    private static QcInspectionItem CreateQcInspectionItem()
    {
        var ctor = typeof(QcInspectionItem).GetConstructor(
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic,
            null,
            Type.EmptyTypes,
            null)!;

        var item = (QcInspectionItem)ctor.Invoke(null);
        TestEntityFactory.SetProperty(item, "ItemName", "外观检查");
        return item;
    }

    #endregion
}
