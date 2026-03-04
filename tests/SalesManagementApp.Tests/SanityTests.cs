using NUnit.Framework;

namespace SalesManagementApp.Tests;

/// <summary>
/// Sanity の仕様を検証するNUnitテストクラスです。
/// </summary>
public class SanityTests
{
    [Test]
    /// <summary>
    /// テストケース「Test Infrastructure Should Pass」の期待結果を検証します。
    /// </summary>
    public void TestInfrastructure_ShouldPass()
    {
        Assert.That(1 + 1, Is.EqualTo(2));
    }

    [Test]
    /// <summary>
    /// テストケース「Core Project Reference Should Be Available」の期待結果を検証します。
    /// </summary>
    public void CoreProjectReference_ShouldBeAvailable()
    {
        var wProduct = new SalesManagementApp.Core.Domain.Entities.Product
        {
            ProductId = "P001",
            ProductName = "Sample",
            UnitPrice = 100,
            Category = "Drink"
        };
        Assert.That(wProduct.ProductId, Is.EqualTo("P001"));
    }
}
