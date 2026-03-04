using System;

namespace SalesManagementApp.Core.Application.Models;

/// <summary>
/// 商品別売上の集計結果を表すモデルです。
/// </summary>
public class ProductSalesSummary
{
    /// <summary>
    /// 集計対象の商品を識別するIDです。
    /// </summary>
    public string ProductId { get; set; } = string.Empty;
    /// <summary>
    /// 対象期間に販売した合計数量です。
    /// </summary>
    public int TotalQuantity { get; set; }
    /// <summary>
    /// 対象期間の合計売上金額です。
    /// </summary>
    public int TotalSalesAmount { get; set; }
}

/// <summary>
/// 週別売上の集計結果を表すモデルです。
/// </summary>
public class WeeklySalesSummary
{
    /// <summary>
    /// 集計対象週の開始日です。
    /// </summary>
    public DateTime WeekStartDate { get; set; }
    /// <summary>
    /// 集計対象週の終了日です。
    /// </summary>
    public DateTime WeekEndDate { get; set; }
    /// <summary>
    /// 当該週に販売した合計数量です。
    /// </summary>
    public int TotalQuantity { get; set; }
    /// <summary>
    /// 当該週の合計売上金額です。
    /// </summary>
    public int TotalSalesAmount { get; set; }
}
