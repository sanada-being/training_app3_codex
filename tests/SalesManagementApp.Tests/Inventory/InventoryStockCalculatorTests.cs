using NUnit.Framework;
using SalesManagementApp.Core.Application.Exceptions;
using SalesManagementApp.Core.Application.Services;

namespace SalesManagementApp.Tests.Inventory;

/// <summary>
/// InventoryStockCalculator の仕様を検証するNUnitテストクラスです。
/// </summary>
public class InventoryStockCalculatorTests
{
    private InventoryStockCalculator FCalculator = null!;

    [SetUp]
    /// <summary>
    /// 公開メソッドです。
    /// </summary>
    public void SetUp()
    {
        FCalculator = new InventoryStockCalculator();
    }

    [Test]
    /// <summary>
    /// 公開メソッドです。
    /// </summary>
    public void CalculateAfterInbound_WhenInputsAreValid_ReturnsIncreasedStock()
    {
        var result = FCalculator.CalculateAfterInbound(10, 5);

        Assert.That(result, Is.EqualTo(15));
    }

    [Test]
    /// <summary>
    /// 公開メソッドです。
    /// </summary>
    public void CalculateAfterInbound_WhenResultOverflows_ThrowsValidationException()
    {
        Assert.That(
            () => FCalculator.CalculateAfterInbound(int.MaxValue, 1),
            Throws.TypeOf<DomainValidationException>());
    }

    [Test]
    /// <summary>
    /// 公開メソッドです。
    /// </summary>
    public void CalculateAfterOutbound_WhenInputsAreValid_ReturnsDecreasedStock()
    {
        var result = FCalculator.CalculateAfterOutbound(10, 4);

        Assert.That(result, Is.EqualTo(6));
    }

    [Test]
    /// <summary>
    /// 公開メソッドです。
    /// </summary>
    public void CalculateAfterOutbound_WhenStockIsInsufficient_ThrowsValidationException()
    {
        Assert.That(
            () => FCalculator.CalculateAfterOutbound(3, 4),
            Throws.TypeOf<DomainValidationException>());
    }
}
