namespace SalesManagementApp.Tests.TestHelpers;

/// <summary>
/// ProductBuilder クラスです。
/// </summary>
public class ProductBuilder
{
    private readonly SalesManagementApp.Core.Domain.Entities.Product FProduct = new()
    {
        ProductId = "P001",
        ProductName = "Cola",
        UnitPrice = 120,
        Category = "Drink"
    };

    /// <summary>
    /// 公開メソッドです。
    /// </summary>
    public ProductBuilder WithId(string value)
    {
        FProduct.ProductId = value;
        return this;
    }

    /// <summary>
    /// 公開メソッドです。
    /// </summary>
    public ProductBuilder WithName(string value)
    {
        FProduct.ProductName = value;
        return this;
    }

    /// <summary>
    /// 公開メソッドです。
    /// </summary>
    public ProductBuilder WithPrice(int value)
    {
        FProduct.UnitPrice = value;
        return this;
    }

    /// <summary>
    /// 公開メソッドです。
    /// </summary>
    public ProductBuilder WithCategory(string value)
    {
        FProduct.Category = value;
        return this;
    }

    /// <summary>
    /// 公開メソッドです。
    /// </summary>
    public SalesManagementApp.Core.Domain.Entities.Product Build() => FProduct;
}
