using System;

namespace SalesManagementApp.Core.Domain.Entities;

/// <summary>
/// 在庫操作履歴の1件を表すドメインエンティティです。
/// </summary>
public class InventoryHistoryRecord
{
    /// <summary>
    /// 公開プロパティです。
    /// </summary>
    public DateTime OccurredAt { get; set; }

    /// <summary>
    /// 公開プロパティです。
    /// </summary>
    public InventoryOperationType OperationType { get; set; }

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
    public int ResultStock { get; set; }

    /// <summary>
    /// 公開プロパティです。
    /// </summary>
    public string Result { get; set; } = string.Empty;
}
