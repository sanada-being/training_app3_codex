namespace SalesManagementApp.Core.Domain.Entities;

/// <summary>
/// InventoryRecord クラスです。
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
