using System.Collections.Generic;
using SalesManagementApp.Core.Domain.Entities;

namespace SalesManagementApp.Core.Application.State;

/// <summary>
/// 画面間で共有するアプリケーション状態を保持します。
/// </summary>
public sealed class AppState
{
    /// <summary>
    /// 画面で参照・更新する商品マスタ一覧です。
    /// </summary>
    public List<Product> Products { get; set; } = new();

    /// <summary>
    /// 最新状態として保持する在庫一覧です。
    /// </summary>
    public List<InventoryRecord> Inventories { get; set; } = new();

    /// <summary>
    /// 在庫操作履歴として保持するレコード一覧です。
    /// </summary>
    public List<InventoryHistoryRecord> InventoryHistories { get; set; } = new();

    /// <summary>
    /// 売上管理で利用する売上レコード一覧です。
    /// </summary>
    public List<SaleRecord> Sales { get; set; } = new();

    /// <summary>
    /// CSVファイル群を配置したリポジトリルートの絶対パスです。
    /// </summary>
    public string RepositoryRootPath { get; set; } = string.Empty;

    /// <summary>
    /// 商品マスタCSVの保存先ファイルパスです。
    /// </summary>
    public string ProductsPath { get; set; } = string.Empty;

    /// <summary>
    /// 在庫CSVの保存先ファイルパスです。
    /// </summary>
    public string InventoryPath { get; set; } = string.Empty;

    /// <summary>
    /// 売上CSVの保存先ファイルパスです。
    /// </summary>
    public string SalesPath { get; set; } = string.Empty;

    /// <summary>
    /// 在庫履歴CSVの保存先ファイルパスです。
    /// </summary>
    public string InventoryHistoryPath { get; set; } = string.Empty;
}
