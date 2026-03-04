using SalesManagementApp.Core.Domain.Entities;

namespace SalesManagementApp.Tests.TestHelpers;

/// <summary>
/// テストデータを組み立てるためのビルダークラスです。
/// </summary>
public class SaleRecordBuilder
{
    private readonly SaleRecord FRecord = new()
    {
        SaleDate = new DateTime(2026, 1, 1),
        StoreId = "S001",
        ProductId = "P001",
        Quantity = 2,
        SalesAmount = 240
    };

    /// <summary>
    /// テストデータの Date を設定し、ビルダー自身を返します。
    /// </summary>
    public SaleRecordBuilder WithDate(DateTime value)
    {
        FRecord.SaleDate = value;
        return this;
    }

    /// <summary>
    /// テストデータの StoreId を設定し、ビルダー自身を返します。
    /// </summary>
    public SaleRecordBuilder WithStoreId(string value)
    {
        FRecord.StoreId = value;
        return this;
    }

    /// <summary>
    /// テストデータの ProductId を設定し、ビルダー自身を返します。
    /// </summary>
    public SaleRecordBuilder WithProductId(string value)
    {
        FRecord.ProductId = value;
        return this;
    }

    /// <summary>
    /// テストデータの Quantity を設定し、ビルダー自身を返します。
    /// </summary>
    public SaleRecordBuilder WithQuantity(int value)
    {
        FRecord.Quantity = value;
        return this;
    }

    /// <summary>
    /// テストデータの SalesAmount を設定し、ビルダー自身を返します。
    /// </summary>
    public SaleRecordBuilder WithSalesAmount(int value)
    {
        FRecord.SalesAmount = value;
        return this;
    }

    /// <summary>
    /// 設定済みの値からテスト用データを生成して返します。
    /// </summary>
    public SaleRecord Build() => FRecord;
}
