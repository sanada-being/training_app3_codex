using NUnit.Framework;
using SalesManagementApp.Core.Application.Exceptions;
using SalesManagementApp.Core.Application.Services;

namespace SalesManagementApp.Tests.Inventory;

public class InventoryStockCalculatorTests
{
    private InventoryStockCalculator _calculator = null!;

    [SetUp]
    public void SetUp()
    {
        _calculator = new InventoryStockCalculator();
    }

    [Test]
    public void CalculateAfterInbound_WhenInputsAreValid_ReturnsIncreasedStock()
    {
        var result = _calculator.CalculateAfterInbound(10, 5);

        Assert.That(result, Is.EqualTo(15));
    }

    [Test]
    public void CalculateAfterInbound_WhenResultOverflows_ThrowsValidationException()
    {
        Assert.That(
            () => _calculator.CalculateAfterInbound(int.MaxValue, 1),
            Throws.TypeOf<DomainValidationException>());
    }

    [Test]
    public void CalculateAfterOutbound_WhenInputsAreValid_ReturnsDecreasedStock()
    {
        var result = _calculator.CalculateAfterOutbound(10, 4);

        Assert.That(result, Is.EqualTo(6));
    }

    [Test]
    public void CalculateAfterOutbound_WhenStockIsInsufficient_ThrowsValidationException()
    {
        Assert.That(
            () => _calculator.CalculateAfterOutbound(3, 4),
            Throws.TypeOf<DomainValidationException>());
    }
}
