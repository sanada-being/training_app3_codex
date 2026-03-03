namespace SalesManagementApp.Tests.TestHelpers;

public class ProductBuilder
{
    private readonly SalesManagementApp.Core.Domain.Entities.Product _product = new()
    {
        ProductId = "P001",
        ProductName = "Cola",
        UnitPrice = 120,
        Category = "Drink"
    };

    public ProductBuilder WithId(string value)
    {
        _product.ProductId = value;
        return this;
    }

    public ProductBuilder WithName(string value)
    {
        _product.ProductName = value;
        return this;
    }

    public ProductBuilder WithPrice(int value)
    {
        _product.UnitPrice = value;
        return this;
    }

    public ProductBuilder WithCategory(string value)
    {
        _product.Category = value;
        return this;
    }

    public SalesManagementApp.Core.Domain.Entities.Product Build() => _product;
}
