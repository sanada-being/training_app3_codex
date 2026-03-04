namespace SalesManagementApp.Core.Domain.Entities;

/// <summary>
/// 店舗別の商品在庫を表すドメインエンティティです。
/// </summary>
public class InventoryRecord
{
    /// <summary>
    /// 在庫を保有する店舗を識別するIDです。
    /// </summary>
    public string StoreId { get; set; } = string.Empty;
    /// <summary>
    /// 在庫対象の商品を識別するIDです。
    /// </summary>
    public string ProductId { get; set; } = string.Empty;
    /// <summary>
    /// 店舗と商品の組み合わせに対する現在在庫数です。
    /// </summary>
    public int Stock { get; set; }
}
