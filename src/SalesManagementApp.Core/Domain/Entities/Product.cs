namespace SalesManagementApp.Core.Domain.Entities;

/// <summary>
/// 商品マスタを表すドメインエンティティです。
/// </summary>
public class Product
{
    /// <summary>
    /// 公開プロパティです。
    /// </summary>
    public string ProductId { get; set; } = string.Empty;
    /// <summary>
    /// 公開プロパティです。
    /// </summary>
    public string ProductName { get; set; } = string.Empty;
    /// <summary>
    /// 公開プロパティです。
    /// </summary>
    public int UnitPrice { get; set; }
    /// <summary>
    /// 公開プロパティです。
    /// </summary>
    public string Category { get; set; } = string.Empty;
}
