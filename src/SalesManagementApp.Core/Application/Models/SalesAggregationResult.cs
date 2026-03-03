using System;

namespace SalesManagementApp.Core.Application.Models;

public class ProductSalesSummary
{
    public string ProductId { get; set; } = string.Empty;
    public int TotalQuantity { get; set; }
    public int TotalSalesAmount { get; set; }
}

public class WeeklySalesSummary
{
    public DateTime WeekStartDate { get; set; }
    public DateTime WeekEndDate { get; set; }
    public int TotalQuantity { get; set; }
    public int TotalSalesAmount { get; set; }
}
