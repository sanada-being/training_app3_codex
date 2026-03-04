using SalesManagementApp.Core.Domain.Entities;

namespace SalesManagementApp.Tests.TestHelpers;

/// <summary>
/// テストデータを組み立てるためのビルダークラスです。
/// </summary>
public class InventoryRecordBuilder
{
    private readonly InventoryRecord FRecord = new()
    {
        StoreId = "S001",
        ProductId = "P001",
        Stock = 10
    };

    /// <summary>
    /// テストデータの StoreId を設定し、ビルダー自身を返します。
    /// </summary>
    public InventoryRecordBuilder WithStoreId(string value)
    {
        FRecord.StoreId = value;
        return this;
    }

    /// <summary>
    /// テストデータの ProductId を設定し、ビルダー自身を返します。
    /// </summary>
    public InventoryRecordBuilder WithProductId(string value)
    {
        FRecord.ProductId = value;
        return this;
    }

    /// <summary>
    /// テストデータの Stock を設定し、ビルダー自身を返します。
    /// </summary>
    public InventoryRecordBuilder WithStock(int value)
    {
        FRecord.Stock = value;
        return this;
    }

    /// <summary>
    /// 設定済みの値からテスト用データを生成して返します。
    /// </summary>
    public InventoryRecord Build() => FRecord;
}
