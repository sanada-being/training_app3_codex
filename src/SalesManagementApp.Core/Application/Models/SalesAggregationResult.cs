using System;

namespace SalesManagementApp.Core.Application.Models;

/// <summary>
/// ProductSalesSummary クラスです。
/// </summary>
public class ProductSalesSummary
{
    /// <summary>
    /// 公開プロパティです。
    /// </summary>
    public string ProductId { get; set; } = string.Empty;
    /// <summary>
    /// 公開プロパティです。
    /// </summary>
    public int TotalQuantity { get; set; }
    /// <summary>
    /// 公開プロパティです。
    /// </summary>
    public int TotalSalesAmount { get; set; }
}

/// <summary>
/// WeeklySalesSummary クラスです。
/// </summary>
public class WeeklySalesSummary
{
    /// <summary>
    /// 公開プロパティです。
    /// </summary>
    public DateTime WeekStartDate { get; set; }
    /// <summary>
    /// 公開プロパティです。
    /// </summary>
    public DateTime WeekEndDate { get; set; }
    /// <summary>
    /// 公開プロパティです。
    /// </summary>
    public int TotalQuantity { get; set; }
    /// <summary>
    /// 公開プロパティです。
    /// </summary>
    public int TotalSalesAmount { get; set; }
}
