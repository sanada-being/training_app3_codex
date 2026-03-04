using NUnit.Framework;

namespace SalesManagementApp.Tests;

/// <summary>
/// Sanity の仕様を検証するNUnitテストクラスです。
/// </summary>
public class SanityTests
{
    [Test]
    /// <summary>
    /// 公開メソッドです。
    /// </summary>
    public void TestInfrastructure_ShouldPass()
    {
        Assert.That(1 + 1, Is.EqualTo(2));
    }

    [Test]
    /// <summary>
    /// 公開メソッドです。
    /// </summary>
    public void CoreProjectReference_ShouldBeAvailable()
    {
        var product = new SalesManagementApp.Core.Domain.Entities.Product
        {
            ProductId = "P001",
            ProductName = "Sample",
            UnitPrice = 100,
            Category = "Drink"
        };
        Assert.That(product.ProductId, Is.EqualTo("P001"));
    }
}
