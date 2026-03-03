namespace SalesManagementApp.Core.Domain.Entities;

public class InventoryRecord
{
    public string StoreId { get; set; } = string.Empty;
    public string ProductId { get; set; } = string.Empty;
    public int Stock { get; set; }
}
