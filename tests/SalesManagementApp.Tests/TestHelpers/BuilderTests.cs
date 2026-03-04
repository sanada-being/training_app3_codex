using NUnit.Framework;
using SalesManagementApp.Tests.TestHelpers;

namespace SalesManagementApp.Tests.TestHelpersTests;

/// <summary>
/// テストデータビルダーの振る舞いを検証するNUnitテストです。
/// </summary>
public class BuilderTests
{
    [Test]
    /// <summary>
    /// テストケース「Product Builder With Price Reflects Specified Value」の期待結果を検証します。
    /// </summary>
    public void ProductBuilder_WithPrice_ReflectsSpecifiedValue()
    {
        var product = new ProductBuilder().WithPrice(500).Build();
        Assert.That(product.UnitPrice, Is.EqualTo(500));
    }

    [Test]
    /// <summary>
    /// テストケース「Inventory Record Builder With Stock Reflects Specified Value」の期待結果を検証します。
    /// </summary>
    public void InventoryRecordBuilder_WithStock_ReflectsSpecifiedValue()
    {
        var record = new InventoryRecordBuilder().WithStock(0).Build();
        Assert.That(record.Stock, Is.EqualTo(0));
    }

    [Test]
    /// <summary>
    /// テストケース「Sale Record Builder With Quantity Reflects Specified Value」の期待結果を検証します。
    /// </summary>
    public void SaleRecordBuilder_WithQuantity_ReflectsSpecifiedValue()
    {
        var sale = new SaleRecordBuilder().WithQuantity(5).Build();
        Assert.That(sale.Quantity, Is.EqualTo(5));
    }
}
