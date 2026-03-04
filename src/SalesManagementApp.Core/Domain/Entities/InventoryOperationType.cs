namespace SalesManagementApp.Core.Domain.Entities;

/// <summary>
/// 在庫履歴で使用する操作種別を定義します。
/// </summary>
public enum InventoryOperationType
{
    /// <summary>
    /// 入庫処理による在庫増加です。
    /// </summary>
    Inbound = 1,
    /// <summary>
    /// 出庫処理による在庫減少です。
    /// </summary>
    Outbound = 2,
    /// <summary>
    /// 売上計上に伴う在庫減少です。
    /// </summary>
    Sale = 3
}
