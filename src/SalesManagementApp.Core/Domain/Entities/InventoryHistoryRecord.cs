using System;

namespace SalesManagementApp.Core.Domain.Entities;

/// <summary>
/// 在庫操作履歴の1件を表すドメインエンティティです。
/// </summary>
public class InventoryHistoryRecord
{
    /// <summary>
    /// 在庫操作が実行された日時です。
    /// </summary>
    public DateTime OccurredAt { get; set; }

    /// <summary>
    /// 実行した在庫操作の種別です。
    /// </summary>
    public InventoryOperationTypeEnum OperationType { get; set; }

    /// <summary>
    /// 操作対象となった店舗を識別するIDです。
    /// </summary>
    public string StoreId { get; set; } = string.Empty;

    /// <summary>
    /// 操作対象となった商品を識別するIDです。
    /// </summary>
    public string ProductId { get; set; } = string.Empty;

    /// <summary>
    /// 操作で増減させた数量です。
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// 操作完了後の在庫数です。
    /// </summary>
    public int ResultStock { get; set; }

    /// <summary>
    /// 在庫操作の成否を示す結果文字列です。
    /// </summary>
    public string Result { get; set; } = string.Empty;
}

