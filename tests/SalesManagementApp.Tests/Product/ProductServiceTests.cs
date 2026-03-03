using System.Collections.Generic;
using NUnit.Framework;
using SalesManagementApp.Core.Application.Exceptions;
using SalesManagementApp.Core.Application.Services;
using SalesManagementApp.Tests.TestHelpers;

namespace SalesManagementApp.Tests.Product;

public class ProductServiceTests
{
    private ProductService _service = null!;
    private List<SalesManagementApp.Core.Domain.Entities.Product> _products = null!;

    [SetUp]
    public void SetUp()
    {
        _service = new ProductService();
        _products = new List<SalesManagementApp.Core.Domain.Entities.Product>();
    }

    [Test]
    public void Register_WhenInputIsValid_AddsProduct()
    {
        var product = new ProductBuilder().WithId("P100").Build();

        _service.Register(_products, product);

        Assert.That(_products.Count, Is.EqualTo(1));
        Assert.That(_products[0].ProductId, Is.EqualTo("P100"));
    }

    [Test]
    public void Register_WhenProductIdIsDuplicate_ThrowsValidationException()
    {
        _products.Add(new ProductBuilder().WithId("P001").Build());
        var duplicate = new ProductBuilder().WithId("P001").Build();

        Assert.That(() => _service.Register(_products, duplicate), Throws.TypeOf<DomainValidationException>());
    }

    [Test]
    public void Register_WhenProductIdHasTrailingSpaceAndDuplicateExists_ThrowsValidationException()
    {
        _products.Add(new ProductBuilder().WithId("P001").Build());
        var duplicate = new ProductBuilder().WithId("P001 ").Build();

        Assert.That(() => _service.Register(_products, duplicate), Throws.TypeOf<DomainValidationException>());
    }

    [Test]
    public void Register_WhenProductNameIsDuplicateAfterTrim_ThrowsValidationException()
    {
        _products.Add(new ProductBuilder().WithId("P001").WithName("Coffee").Build());
        var duplicate = new ProductBuilder().WithId("P002").WithName("  Coffee  ").Build();

        Assert.That(() => _service.Register(_products, duplicate), Throws.TypeOf<DomainValidationException>());
    }

    [Test]
    public void Register_WhenProductNameDiffersOnlyByCase_ThrowsValidationException()
    {
        _products.Add(new ProductBuilder().WithId("P001").WithName("Coffee").Build());
        var duplicate = new ProductBuilder().WithId("P002").WithName("coffee").Build();

        Assert.That(() => _service.Register(_products, duplicate), Throws.TypeOf<DomainValidationException>());
    }

    [Test]
    public void Register_WhenProductNameDiffersOnlyByFullHalfWidth_ThrowsValidationException()
    {
        _products.Add(new ProductBuilder().WithId("P001").WithName("ｺｰﾋｰ").Build());
        var duplicate = new ProductBuilder().WithId("P002").WithName("コーヒー").Build();

        Assert.That(() => _service.Register(_products, duplicate), Throws.TypeOf<DomainValidationException>());
    }

    [Test]
    public void Register_WhenPriceIsNegative_ThrowsValidationException()
    {
        var invalid = new ProductBuilder().WithPrice(-1).Build();

        Assert.That(() => _service.Register(_products, invalid), Throws.TypeOf<DomainValidationException>());
    }

    [Test]
    public void Update_WhenProductExists_UpdatesFields()
    {
        _products.Add(new ProductBuilder().WithId("P001").WithName("Old").Build());
        var update = new ProductBuilder().WithId("P001").WithName("New").WithPrice(999).WithCategory("Snack").Build();

        _service.Update(_products, update);

        Assert.That(_products[0].ProductName, Is.EqualTo("New"));
        Assert.That(_products[0].UnitPrice, Is.EqualTo(999));
        Assert.That(_products[0].Category, Is.EqualTo("Snack"));
    }

    [Test]
    public void Update_WhenProductIdHasTrailingSpace_FindsAndUpdatesTarget()
    {
        _products.Add(new ProductBuilder().WithId("P001").WithName("Old").Build());
        var update = new ProductBuilder().WithId("P001 ").WithName("New").WithPrice(999).WithCategory("Snack").Build();

        _service.Update(_products, update);

        Assert.That(_products[0].ProductId, Is.EqualTo("P001"));
        Assert.That(_products[0].ProductName, Is.EqualTo("New"));
        Assert.That(_products[0].UnitPrice, Is.EqualTo(999));
        Assert.That(_products[0].Category, Is.EqualTo("Snack"));
    }

    [Test]
    public void Update_WhenProductNameDuplicatesAnotherProduct_ThrowsValidationException()
    {
        _products.Add(new ProductBuilder().WithId("P001").WithName("Coffee").Build());
        _products.Add(new ProductBuilder().WithId("P002").WithName("Tea").Build());
        var update = new ProductBuilder().WithId("P002").WithName(" coffee ").Build();

        Assert.That(() => _service.Update(_products, update), Throws.TypeOf<DomainValidationException>());
    }

    [Test]
    public void Delete_WhenProductExists_RemovesProduct()
    {
        _products.Add(new ProductBuilder().WithId("P001").Build());

        _service.Delete(_products, "P001");

        Assert.That(_products, Is.Empty);
    }

    [Test]
    public void GetFiltered_WhenConditionsMatch_ReturnsFilteredProducts()
    {
        _products.Add(new ProductBuilder().WithId("P001").WithName("Cola").WithCategory("Drink").Build());
        _products.Add(new ProductBuilder().WithId("P002").WithName("Tea").WithCategory("Drink").Build());
        _products.Add(new ProductBuilder().WithId("P003").WithName("Bread").WithCategory("Food").Build());

        var filtered = _service.GetFiltered(_products, "P00", "ea", "Drink");

        Assert.That(filtered.Count, Is.EqualTo(1));
        Assert.That(filtered[0].ProductId, Is.EqualTo("P002"));
    }
}
