using SalesManagementApp.Core.Domain.Entities;

namespace SalesManagementApp.Tests.TestHelpers;

public class SaleRecordBuilder
{
    private readonly SaleRecord _record = new()
    {
        SaleDate = new DateTime(2026, 1, 1),
        StoreId = "S001",
        ProductId = "P001",
        Quantity = 2,
        SalesAmount = 240
    };

    public SaleRecordBuilder WithDate(DateTime value)
    {
        _record.SaleDate = value;
        return this;
    }

    public SaleRecordBuilder WithStoreId(string value)
    {
        _record.StoreId = value;
        return this;
    }

    public SaleRecordBuilder WithProductId(string value)
    {
        _record.ProductId = value;
        return this;
    }

    public SaleRecordBuilder WithQuantity(int value)
    {
        _record.Quantity = value;
        return this;
    }

    public SaleRecordBuilder WithSalesAmount(int value)
    {
        _record.SalesAmount = value;
        return this;
    }

    public SaleRecord Build() => _record;
}
