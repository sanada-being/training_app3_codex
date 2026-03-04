namespace SalesManagementApp.Core.Domain.Entities;

/// <summary>
/// 店舗別の商品在庫を表すドメインエンティティです。
/// </summary>
public class InventoryRecord
{
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
    public int Stock { get; set; }
}
