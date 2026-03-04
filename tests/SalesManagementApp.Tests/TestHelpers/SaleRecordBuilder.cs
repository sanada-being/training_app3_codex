using SalesManagementApp.Core.Domain.Entities;

namespace SalesManagementApp.Tests.TestHelpers;

/// <summary>
/// SaleRecordBuilder クラスです。
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
    /// 公開メソッドです。
    /// </summary>
    public SaleRecordBuilder WithDate(DateTime value)
    {
        FRecord.SaleDate = value;
        return this;
    }

    /// <summary>
    /// 公開メソッドです。
    /// </summary>
    public SaleRecordBuilder WithStoreId(string value)
    {
        FRecord.StoreId = value;
        return this;
    }

    /// <summary>
    /// 公開メソッドです。
    /// </summary>
    public SaleRecordBuilder WithProductId(string value)
    {
        FRecord.ProductId = value;
        return this;
    }

    /// <summary>
    /// 公開メソッドです。
    /// </summary>
    public SaleRecordBuilder WithQuantity(int value)
    {
        FRecord.Quantity = value;
        return this;
    }

    /// <summary>
    /// 公開メソッドです。
    /// </summary>
    public SaleRecordBuilder WithSalesAmount(int value)
    {
        FRecord.SalesAmount = value;
        return this;
    }

    /// <summary>
    /// 公開メソッドです。
    /// </summary>
    public SaleRecord Build() => FRecord;
}
