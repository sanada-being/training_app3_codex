using System.Collections.Generic;
using NUnit.Framework;
using SalesManagementApp.Core.Application.Exceptions;
using SalesManagementApp.Core.Application.Services;
using SalesManagementApp.Tests.TestHelpers;

namespace SalesManagementApp.Tests.Product;

/// <summary>
/// ProductService の仕様を検証するNUnitテストクラスです。
/// </summary>
public class ProductServiceTests
{
    private ProductService FService = null!;
    private List<SalesManagementApp.Core.Domain.Entities.Product> FProducts = null!;

    [SetUp]
    /// <summary>
    /// 公開メソッドです。
    /// </summary>
    public void SetUp()
    {
        FService = new ProductService();
        FProducts = new List<SalesManagementApp.Core.Domain.Entities.Product>();
    }

    [Test]
    /// <summary>
    /// 公開メソッドです。
    /// </summary>
    public void Register_WhenInputIsValid_AddsProduct()
    {
        var product = new ProductBuilder().WithId("P100").Build();

        FService.Register(FProducts, product);

        Assert.That(FProducts.Count, Is.EqualTo(1));
        Assert.That(FProducts[0].ProductId, Is.EqualTo("P100"));
    }

    [Test]
    /// <summary>
    /// 公開メソッドです。
    /// </summary>
    public void Register_WhenProductIdIsDuplicate_ThrowsValidationException()
    {
        FProducts.Add(new ProductBuilder().WithId("P001").Build());
        var duplicate = new ProductBuilder().WithId("P001").Build();

        Assert.That(() => FService.Register(FProducts, duplicate), Throws.TypeOf<DomainValidationException>());
    }

    [Test]
    /// <summary>
    /// 公開メソッドです。
    /// </summary>
    public void Register_WhenProductIdHasTrailingSpaceAndDuplicateExists_ThrowsValidationException()
    {
        FProducts.Add(new ProductBuilder().WithId("P001").Build());
        var duplicate = new ProductBuilder().WithId("P001 ").Build();

        Assert.That(() => FService.Register(FProducts, duplicate), Throws.TypeOf<DomainValidationException>());
    }

    [Test]
    /// <summary>
    /// 公開メソッドです。
    /// </summary>
    public void Register_WhenProductNameIsDuplicateAfterTrim_ThrowsValidationException()
    {
        FProducts.Add(new ProductBuilder().WithId("P001").WithName("Coffee").Build());
        var duplicate = new ProductBuilder().WithId("P002").WithName("  Coffee  ").Build();

        Assert.That(() => FService.Register(FProducts, duplicate), Throws.TypeOf<DomainValidationException>());
    }

    [Test]
    /// <summary>
    /// 公開メソッドです。
    /// </summary>
    public void Register_WhenProductNameDiffersOnlyByCase_ThrowsValidationException()
    {
        FProducts.Add(new ProductBuilder().WithId("P001").WithName("Coffee").Build());
        var duplicate = new ProductBuilder().WithId("P002").WithName("coffee").Build();

        Assert.That(() => FService.Register(FProducts, duplicate), Throws.TypeOf<DomainValidationException>());
    }

    [Test]
    /// <summary>
    /// 公開メソッドです。
    /// </summary>
    public void Register_WhenProductNameDiffersOnlyByFullHalfWidth_ThrowsValidationException()
    {
        FProducts.Add(new ProductBuilder().WithId("P001").WithName("ｺｰﾋｰ").Build());
        var duplicate = new ProductBuilder().WithId("P002").WithName("コーヒー").Build();

        Assert.That(() => FService.Register(FProducts, duplicate), Throws.TypeOf<DomainValidationException>());
    }

    [Test]
    /// <summary>
    /// 公開メソッドです。
    /// </summary>
    public void Register_WhenPriceIsNegative_ThrowsValidationException()
    {
        var invalid = new ProductBuilder().WithPrice(-1).Build();

        Assert.That(() => FService.Register(FProducts, invalid), Throws.TypeOf<DomainValidationException>());
    }

    [Test]
    /// <summary>
    /// 公開メソッドです。
    /// </summary>
    public void Update_WhenProductExists_UpdatesFields()
    {
        FProducts.Add(new ProductBuilder().WithId("P001").WithName("Old").Build());
        var update = new ProductBuilder().WithId("P001").WithName("New").WithPrice(999).WithCategory("Snack").Build();

        FService.Update(FProducts, update);

        Assert.That(FProducts[0].ProductName, Is.EqualTo("New"));
        Assert.That(FProducts[0].UnitPrice, Is.EqualTo(999));
        Assert.That(FProducts[0].Category, Is.EqualTo("Snack"));
    }

    [Test]
    /// <summary>
    /// 公開メソッドです。
    /// </summary>
    public void Update_WhenProductIdHasTrailingSpace_FindsAndUpdatesTarget()
    {
        FProducts.Add(new ProductBuilder().WithId("P001").WithName("Old").Build());
        var update = new ProductBuilder().WithId("P001 ").WithName("New").WithPrice(999).WithCategory("Snack").Build();

        FService.Update(FProducts, update);

        Assert.That(FProducts[0].ProductId, Is.EqualTo("P001"));
        Assert.That(FProducts[0].ProductName, Is.EqualTo("New"));
        Assert.That(FProducts[0].UnitPrice, Is.EqualTo(999));
        Assert.That(FProducts[0].Category, Is.EqualTo("Snack"));
    }

    [Test]
    /// <summary>
    /// 公開メソッドです。
    /// </summary>
    public void Update_WhenProductNameDuplicatesAnotherProduct_ThrowsValidationException()
    {
        FProducts.Add(new ProductBuilder().WithId("P001").WithName("Coffee").Build());
        FProducts.Add(new ProductBuilder().WithId("P002").WithName("Tea").Build());
        var update = new ProductBuilder().WithId("P002").WithName(" coffee ").Build();

        Assert.That(() => FService.Update(FProducts, update), Throws.TypeOf<DomainValidationException>());
    }

    [Test]
    /// <summary>
    /// 公開メソッドです。
    /// </summary>
    public void Delete_WhenProductExists_RemovesProduct()
    {
        FProducts.Add(new ProductBuilder().WithId("P001").Build());

        FService.Delete(FProducts, "P001");

        Assert.That(FProducts, Is.Empty);
    }

    [Test]
    /// <summary>
    /// 公開メソッドです。
    /// </summary>
    public void GetFiltered_WhenConditionsMatch_ReturnsFilteredProducts()
    {
        FProducts.Add(new ProductBuilder().WithId("P001").WithName("Cola").WithCategory("Drink").Build());
        FProducts.Add(new ProductBuilder().WithId("P002").WithName("Tea").WithCategory("Drink").Build());
        FProducts.Add(new ProductBuilder().WithId("P003").WithName("Bread").WithCategory("Food").Build());

        var filtered = FService.GetFiltered(FProducts, "P00", "ea", "Drink");

        Assert.That(filtered.Count, Is.EqualTo(1));
        Assert.That(filtered[0].ProductId, Is.EqualTo("P002"));
    }
}
