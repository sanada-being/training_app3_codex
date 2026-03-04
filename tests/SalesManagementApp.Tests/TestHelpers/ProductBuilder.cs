namespace SalesManagementApp.Tests.TestHelpers;

/// <summary>
/// テストデータを組み立てるためのビルダークラスです。
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
    /// テストデータの Id を設定し、ビルダー自身を返します。
    /// </summary>
    public ProductBuilder WithId(string vValue)
    {
        FProduct.ProductId = vValue;
        return this;
    }

    /// <summary>
    /// テストデータの Name を設定し、ビルダー自身を返します。
    /// </summary>
    public ProductBuilder WithName(string vValue)
    {
        FProduct.ProductName = vValue;
        return this;
    }

    /// <summary>
    /// テストデータの Price を設定し、ビルダー自身を返します。
    /// </summary>
    public ProductBuilder WithPrice(int vValue)
    {
        FProduct.UnitPrice = vValue;
        return this;
    }

    /// <summary>
    /// テストデータの Category を設定し、ビルダー自身を返します。
    /// </summary>
    public ProductBuilder WithCategory(string vValue)
    {
        FProduct.Category = vValue;
        return this;
    }

    /// <summary>
    /// 設定済みの値からテスト用データを生成して返します。
    /// </summary>
    public SalesManagementApp.Core.Domain.Entities.Product Build() => FProduct;
}
