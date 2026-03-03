using System;

namespace SalesManagementApp.Core.Domain.Entities;

public class SaleRecord
{
    public DateTime SaleDate { get; set; }
    public string StoreId { get; set; } = string.Empty;
    public string ProductId { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public int SalesAmount { get; set; }
}
