using SalesManagementApp.Core.Domain.Entities;

namespace SalesManagementApp.Tests.TestHelpers;

public class InventoryRecordBuilder
{
    private readonly InventoryRecord _record = new()
    {
        StoreId = "S001",
        ProductId = "P001",
        Stock = 10
    };

    public InventoryRecordBuilder WithStoreId(string value)
    {
        _record.StoreId = value;
        return this;
    }

    public InventoryRecordBuilder WithProductId(string value)
    {
        _record.ProductId = value;
        return this;
    }

    public InventoryRecordBuilder WithStock(int value)
    {
        _record.Stock = value;
        return this;
    }

    public InventoryRecord Build() => _record;
}
