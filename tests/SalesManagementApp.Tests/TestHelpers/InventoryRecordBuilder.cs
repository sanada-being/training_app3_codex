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
    /// 公開メソッドです。
    /// </summary>
    public InventoryRecordBuilder WithStoreId(string value)
    {
        FRecord.StoreId = value;
        return this;
    }

    /// <summary>
    /// 公開メソッドです。
    /// </summary>
    public InventoryRecordBuilder WithProductId(string value)
    {
        FRecord.ProductId = value;
        return this;
    }

    /// <summary>
    /// 公開メソッドです。
    /// </summary>
    public InventoryRecordBuilder WithStock(int value)
    {
        FRecord.Stock = value;
        return this;
    }

    /// <summary>
    /// 公開メソッドです。
    /// </summary>
    public InventoryRecord Build() => FRecord;
}
