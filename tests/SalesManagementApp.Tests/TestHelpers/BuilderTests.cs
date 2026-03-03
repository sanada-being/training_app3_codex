using NUnit.Framework;
using SalesManagementApp.Tests.TestHelpers;

namespace SalesManagementApp.Tests.TestHelpersTests;

public class BuilderTests
{
    [Test]
    public void ProductBuilder_WithPrice_ReflectsSpecifiedValue()
    {
        var product = new ProductBuilder().WithPrice(500).Build();
        Assert.That(product.UnitPrice, Is.EqualTo(500));
    }

    [Test]
    public void InventoryRecordBuilder_WithStock_ReflectsSpecifiedValue()
    {
        var record = new InventoryRecordBuilder().WithStock(0).Build();
        Assert.That(record.Stock, Is.EqualTo(0));
    }

    [Test]
    public void SaleRecordBuilder_WithQuantity_ReflectsSpecifiedValue()
    {
        var sale = new SaleRecordBuilder().WithQuantity(5).Build();
        Assert.That(sale.Quantity, Is.EqualTo(5));
    }
}
