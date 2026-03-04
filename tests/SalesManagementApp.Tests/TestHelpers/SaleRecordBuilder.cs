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
    public SaleRecordBuilder WithDate(DateTime vValue)
    {
        FRecord.SaleDate = vValue;
        return this;
    }

    /// <summary>
    /// テストデータの StoreId を設定し、ビルダー自身を返します。
    /// </summary>
    public SaleRecordBuilder WithStoreId(string vValue)
    {
        FRecord.StoreId = vValue;
        return this;
    }

    /// <summary>
    /// テストデータの ProductId を設定し、ビルダー自身を返します。
    /// </summary>
    public SaleRecordBuilder WithProductId(string vValue)
    {
        FRecord.ProductId = vValue;
        return this;
    }

    /// <summary>
    /// テストデータの Quantity を設定し、ビルダー自身を返します。
    /// </summary>
    public SaleRecordBuilder WithQuantity(int vValue)
    {
        FRecord.Quantity = vValue;
        return this;
    }

    /// <summary>
    /// テストデータの SalesAmount を設定し、ビルダー自身を返します。
    /// </summary>
    public SaleRecordBuilder WithSalesAmount(int vValue)
    {
        FRecord.SalesAmount = vValue;
        return this;
    }

    /// <summary>
    /// 設定済みの値からテスト用データを生成して返します。
    /// </summary>
    public SaleRecord Build() => FRecord;
}
