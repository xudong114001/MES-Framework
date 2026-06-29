using MES.Domain.Exceptions;
using MES.Domain.ValueObjects;
using Xunit;

namespace MES.Tests.Domain;

public class QuantityTests
{
    #region Constructor 构造函数

    [Fact]
    public void Constructor_SetsValueAndUnit()
    {
        var qty = new Quantity(100, "PCS");

        Assert.Equal(100, qty.Value);
        Assert.Equal("PCS", qty.Unit);
    }

    [Fact]
    public void Constructor_DefaultUnitIsPcs()
    {
        var qty = new Quantity(50);

        Assert.Equal(50, qty.Value);
        Assert.Equal("PCS", qty.Unit);
    }

    [Fact]
    public void Constructor_Throws_WhenValueNegative()
    {
        Assert.Throws<DomainException>(() => new Quantity(-1));
    }

    [Fact]
    public void Constructor_AllowsZero()
    {
        var qty = new Quantity(0);

        Assert.Equal(0, qty.Value);
    }

    [Fact]
    public void Constructor_Throws_WhenUnitEmpty()
    {
        Assert.Throws<DomainException>(() => new Quantity(10, ""));
    }

    [Fact]
    public void Constructor_Throws_WhenUnitWhitespace()
    {
        Assert.Throws<DomainException>(() => new Quantity(10, "   "));
    }

    #endregion

    #region Add 加法

    [Fact]
    public void Add_ReturnsSum_WhenSameUnit()
    {
        var a = new Quantity(30, "PCS");
        var b = new Quantity(20, "PCS");

        var result = a.Add(b);

        Assert.Equal(50, result.Value);
        Assert.Equal("PCS", result.Unit);
    }

    [Fact]
    public void Add_Throws_WhenDifferentUnits()
    {
        var a = new Quantity(30, "PCS");
        var b = new Quantity(20, "KG");

        Assert.Throws<DomainException>(() => a.Add(b));
    }

    #endregion

    #region Subtract 减法

    [Fact]
    public void Subtract_ReturnsDifference_WhenSameUnit()
    {
        var a = new Quantity(50, "PCS");
        var b = new Quantity(20, "PCS");

        var result = a.Subtract(b);

        Assert.Equal(30, result.Value);
        Assert.Equal("PCS", result.Unit);
    }

    [Fact]
    public void Subtract_Throws_WhenDifferentUnits()
    {
        var a = new Quantity(50, "PCS");
        var b = new Quantity(20, "KG");

        Assert.Throws<DomainException>(() => a.Subtract(b));
    }

    [Fact]
    public void Subtract_Throws_WhenResultNegative()
    {
        var a = new Quantity(10, "PCS");
        var b = new Quantity(20, "PCS");

        Assert.Throws<DomainException>(() => a.Subtract(b));
    }

    #endregion

    #region Operator+ / Operator-

    [Fact]
    public void OperatorPlus_ReturnsSum_WhenSameUnit()
    {
        var a = new Quantity(30, "PCS");
        var b = new Quantity(20, "PCS");

        var result = a + b;

        Assert.Equal(50, result.Value);
    }

    [Fact]
    public void OperatorPlus_Throws_WhenDifferentUnits()
    {
        var a = new Quantity(30, "PCS");
        var b = new Quantity(20, "KG");

        Assert.Throws<DomainException>(() => a + b);
    }

    [Fact]
    public void OperatorMinus_ReturnsDifference_WhenSameUnit()
    {
        var a = new Quantity(50, "PCS");
        var b = new Quantity(20, "PCS");

        var result = a - b;

        Assert.Equal(30, result.Value);
    }

    [Fact]
    public void OperatorMinus_Throws_WhenDifferentUnits()
    {
        var a = new Quantity(50, "PCS");
        var b = new Quantity(20, "KG");

        Assert.Throws<DomainException>(() => a - b);
    }

    #endregion

    #region Properties 属性

    [Fact]
    public void IsZero_ReturnsTrue_WhenValueIsZero()
    {
        var qty = new Quantity(0);

        Assert.True(qty.IsZero);
    }

    [Fact]
    public void IsZero_ReturnsFalse_WhenValueIsPositive()
    {
        var qty = new Quantity(10);

        Assert.False(qty.IsZero);
    }

    [Fact]
    public void IsPositive_ReturnsTrue_WhenValueIsPositive()
    {
        var qty = new Quantity(10);

        Assert.True(qty.IsPositive);
    }

    [Fact]
    public void IsPositive_ReturnsFalse_WhenValueIsZero()
    {
        var qty = new Quantity(0);

        Assert.False(qty.IsPositive);
    }

    [Fact]
    public void IsNegative_ReturnsFalse_ForValidQuantity()
    {
        var qty = new Quantity(10);

        Assert.False(qty.IsNegative);
    }

    #endregion

    #region Zero 工厂方法

    [Fact]
    public void Zero_CreatesZeroQuantity_WithDefaultUnit()
    {
        var zero = Quantity.Zero();

        Assert.Equal(0, zero.Value);
        Assert.Equal("PCS", zero.Unit);
    }

    [Fact]
    public void Zero_CreatesZeroQuantity_WithSpecifiedUnit()
    {
        var zero = Quantity.Zero("KG");

        Assert.Equal(0, zero.Value);
        Assert.Equal("KG", zero.Unit);
    }

    #endregion

    #region Implicit conversion 隐式转换

    [Fact]
    public void ImplicitConversion_ToDecimal_ReturnsValue()
    {
        var qty = new Quantity(42.5m, "PCS");
        decimal value = qty;

        Assert.Equal(42.5m, value);
    }

    #endregion

    #region ToString

    [Fact]
    public void ToString_ReturnsFormattedString()
    {
        var qty = new Quantity(100, "PCS");

        Assert.Equal("100 PCS", qty.ToString());
    }

    #endregion
}
