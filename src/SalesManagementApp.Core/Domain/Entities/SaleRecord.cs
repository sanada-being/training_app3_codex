using System;

namespace SalesManagementApp.Core.Domain.Entities;

/// <summary>
/// 売上記録の1件を表すドメインエンティティです。
/// </summary>
public class SaleRecord
{
    /// <summary>
    /// 公開プロパティです。
    /// </summary>
    public DateTime SaleDate { get; set; }
    /// <summary>
    /// 公開プロパティです。
    /// </summary>
    public string StoreId { get; set; } = string.Empty;
    /// <summary>
    /// 公開プロパティです。
    /// </summary>
    public string ProductId { get; set; } = string.Empty;
    /// <summary>
    /// 公開プロパティです。
    /// </summary>
    public int Quantity { get; set; }
    /// <summary>
    /// 公開プロパティです。
    /// </summary>
    public int SalesAmount { get; set; }
}
