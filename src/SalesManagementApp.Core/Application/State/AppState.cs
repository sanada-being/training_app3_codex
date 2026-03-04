using System.Collections.Generic;
using SalesManagementApp.Core.Domain.Entities;

namespace SalesManagementApp.Core.Application.State;

/// <summary>
/// AppState クラスです。
/// </summary>
public sealed class AppState
{
    /// <summary>
    /// 公開メソッドです。
    /// </summary>
    public List<Product> Products { get; set; } = new();

    /// <summary>
    /// 公開メソッドです。
    /// </summary>
    public List<InventoryRecord> Inventories { get; set; } = new();

    /// <summary>
    /// 公開メソッドです。
    /// </summary>
    public List<InventoryHistoryRecord> InventoryHistories { get; set; } = new();

    /// <summary>
    /// 公開メソッドです。
    /// </summary>
    public List<SaleRecord> Sales { get; set; } = new();

    /// <summary>
    /// 公開プロパティです。
    /// </summary>
    public string RepositoryRootPath { get; set; } = string.Empty;

    /// <summary>
    /// 公開プロパティです。
    /// </summary>
    public string ProductsPath { get; set; } = string.Empty;

    /// <summary>
    /// 公開プロパティです。
    /// </summary>
    public string InventoryPath { get; set; } = string.Empty;

    /// <summary>
    /// 公開プロパティです。
    /// </summary>
    public string SalesPath { get; set; } = string.Empty;

    /// <summary>
    /// 公開プロパティです。
    /// </summary>
    public string InventoryHistoryPath { get; set; } = string.Empty;
}
