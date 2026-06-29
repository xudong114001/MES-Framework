using MES.Domain.Entities;
using MES.Domain.Exceptions;
using Xunit;

namespace MES.Tests.Domain;

public class MaterialTests
{
    #region Create 工厂方法

    [Fact]
    public void Create_SetsDefaultValues()
    {
        var material = Material.Create("MAT-001", "Test Material");

        Assert.Equal("MAT-001", material.Code);
        Assert.Equal("Test Material", material.Name);
        Assert.Equal(0, material.StockQty);
        Assert.True(material.Status);
    }

    [Fact]
    public void Create_ThrowsWhenCodeIsEmpty()
    {
        Assert.Throws<DomainException>(() =>
            Material.Create("", "Test Material"));
    }

    [Fact]
    public void Create_ThrowsWhenNameIsEmpty()
    {
        Assert.Throws<DomainException>(() =>
            Material.Create("MAT-001", ""));
    }

    [Fact]
    public void Create_SetsOptionalFields()
    {
        var material = Material.Create("MAT-001", "Test Material",
            spec: "100x200",
            unit: "PCS",
            category: "Component",
            bomLevel: 1);

        Assert.Equal("100x200", material.Spec);
        Assert.Equal("PCS", material.Unit);
        Assert.Equal("Component", material.Category);
        Assert.Equal(1, material.BomLevel);
    }

    #endregion

    #region AdjustStock 库存调整

    [Fact]
    public void AdjustStock_IncreasesStock_WhenPositiveDelta()
    {
        var material = Material.Create("MAT-001", "Test Material");

        material.AdjustStock(100, "入库");

        Assert.Equal(100, material.StockQty);
    }

    [Fact]
    public void AdjustStock_DecreasesStock_WhenNegativeDelta()
    {
        var material = Material.Create("MAT-001", "Test Material");
        material.AdjustStock(100, "入库");

        material.AdjustStock(-30, "出库");

        Assert.Equal(70, material.StockQty);
    }

    [Fact]
    public void AdjustStock_Throws_WhenStockInsufficient()
    {
        var material = Material.Create("MAT-001", "Test Material");
        material.AdjustStock(10, "入库");

        var ex = Assert.Throws<DomainException>(() => material.AdjustStock(-20, "出库"));
        Assert.Contains("库存不足", ex.Message);
    }

    [Fact]
    public void AdjustStock_Throws_WhenReasonEmpty()
    {
        var material = Material.Create("MAT-001", "Test Material");

        Assert.Throws<DomainException>(() => material.AdjustStock(100, ""));
    }

    [Fact]
    public void AdjustStock_Throws_WhenReasonNull()
    {
        var material = Material.Create("MAT-001", "Test Material");

        Assert.Throws<DomainException>(() => material.AdjustStock(100, null!));
    }

    [Fact]
    public void AdjustStock_Throws_WhenMaterialDeactivated()
    {
        var material = Material.Create("MAT-001", "Test Material");
        material.Deactivate();

        Assert.Throws<DomainException>(() => material.AdjustStock(100, "入库"));
    }

    [Fact]
    public void AdjustStock_AllowsZeroDelta()
    {
        var material = Material.Create("MAT-001", "Test Material");
        material.AdjustStock(50, "初始入库");

        material.AdjustStock(0, "无变化");

        Assert.Equal(50, material.StockQty);
    }

    #endregion

    #region Deactivate 停用

    [Fact]
    public void Deactivate_SetsStatusToFalse()
    {
        var material = Material.Create("MAT-001", "Test Material");

        material.Deactivate();

        Assert.False(material.Status);
    }

    [Fact]
    public void Deactivate_Throws_WhenAlreadyDeactivated()
    {
        var material = Material.Create("MAT-001", "Test Material");
        material.Deactivate();

        Assert.Throws<DomainException>(() => material.Deactivate());
    }

    #endregion

    #region Activate 启用

    [Fact]
    public void Activate_SetsStatusToTrue()
    {
        var material = Material.Create("MAT-001", "Test Material");
        material.Deactivate();

        material.Activate();

        Assert.True(material.Status);
    }

    [Fact]
    public void Activate_Throws_WhenAlreadyActive()
    {
        var material = Material.Create("MAT-001", "Test Material");

        Assert.Throws<DomainException>(() => material.Activate());
    }

    #endregion

    #region UpdateSpec 更新规格

    [Fact]
    public void UpdateSpec_UpdatesSpecInfo()
    {
        var material = Material.Create("MAT-001", "Test Material");

        material.UpdateSpec("200x300", "KG", "RawMaterial");

        Assert.Equal("200x300", material.Spec);
        Assert.Equal("KG", material.Unit);
        Assert.Equal("RawMaterial", material.Category);
    }

    [Fact]
    public void UpdateSpec_Throws_WhenDeactivated()
    {
        var material = Material.Create("MAT-001", "Test Material");
        material.Deactivate();

        Assert.Throws<DomainException>(() => material.UpdateSpec("spec", "unit", "cat"));
    }

    #endregion
}
