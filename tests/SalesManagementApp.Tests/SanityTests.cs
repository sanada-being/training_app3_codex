using NUnit.Framework;

namespace SalesManagementApp.Tests;

public class SanityTests
{
    [Test]
    public void TestInfrastructure_ShouldPass()
    {
        Assert.That(1 + 1, Is.EqualTo(2));
    }

    [Test]
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
