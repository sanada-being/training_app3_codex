using System;

namespace SalesManagementApp.Core.Domain.Entities;

/// <summary>
/// 売上記録の1件を表すドメインエンティティです。
/// </summary>
public class SaleRecord
{
    /// <summary>
    /// 売上が発生した日付です。
    /// </summary>
    public DateTime SaleDate { get; set; }
    /// <summary>
    /// 売上を計上した店舗を識別するIDです。
    /// </summary>
    public string StoreId { get; set; } = string.Empty;
    /// <summary>
    /// 売上対象の商品を識別するIDです。
    /// </summary>
    public string ProductId { get; set; } = string.Empty;
    /// <summary>
    /// 売上として計上した販売数量です。
    /// </summary>
    public int Quantity { get; set; }
    /// <summary>
    /// 売上レコードに記録する売上金額です。
    /// </summary>
    public int SalesAmount { get; set; }
}
