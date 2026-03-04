namespace SalesManagementApp.Core.Domain.Entities;

/// <summary>
/// 商品マスタを表すドメインエンティティです。
/// </summary>
public class Product
{
    /// <summary>
    /// 商品を一意に識別するIDです。
    /// </summary>
    public string ProductId { get; set; } = string.Empty;
    /// <summary>
    /// 商品マスタで管理する商品名称です。
    /// </summary>
    public string ProductName { get; set; } = string.Empty;
    /// <summary>
    /// 販売時に使用する商品単価（円）です。
    /// </summary>
    public int UnitPrice { get; set; }
    /// <summary>
    /// 検索や集計に利用する商品カテゴリ名です。
    /// </summary>
    public string Category { get; set; } = string.Empty;
}
