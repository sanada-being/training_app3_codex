namespace SalesManagementApp.Core.Domain.Entities;

public class Product
{
    public string ProductId { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public int UnitPrice { get; set; }
    public string Category { get; set; } = string.Empty;
}
