using System;

namespace SalesManagementApp.Core.Application.Models;

/// <summary>
/// 商品別売上の集計結果を表すモデルです。
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
/// 週別売上の集計結果を表すモデルです。
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
