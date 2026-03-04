using System;
using System.Collections.Generic;
using System.IO;
using SalesManagementApp.Core.Domain.Entities;
using SalesManagementApp.Core.Infrastructure.Csv;

namespace SalesManagementApp.Core.Application.State;

/// <summary>
/// CSVデータの読み書きを通じてアプリ状態を永続化します。
/// </summary>
public class AppDataRepository
{
    private readonly CsvDataStore FCsvDataStore;

    /// <summary>
    /// CSVデータ操作に使用するデータストアを初期化します。
    /// </summary>
    public AppDataRepository()
        : this(new CsvDataStore())
    {
    }

    internal AppDataRepository(CsvDataStore csvDataStore)
    {
        FCsvDataStore = csvDataStore ?? throw new ArgumentNullException(nameof(csvDataStore));
    }

    /// <summary>
    /// 商品CSVが存在する場合に商品一覧を読み込みます。
    /// </summary>
    public IReadOnlyList<Product> ReadProductsIfExists(string filePath)
    {
        return File.Exists(filePath) ? FCsvDataStore.ReadProducts(filePath) : Array.Empty<Product>();
    }

    /// <summary>
    /// 在庫CSVが存在する場合に在庫一覧を読み込みます。
    /// </summary>
    public IReadOnlyList<InventoryRecord> ReadInventoriesIfExists(string filePath)
    {
        return File.Exists(filePath) ? FCsvDataStore.ReadInventories(filePath) : Array.Empty<InventoryRecord>();
    }

    /// <summary>
    /// 売上CSVが存在する場合に正規化を行って読み込みます。
    /// </summary>
    public IReadOnlyList<SaleRecord> ReadAndNormalizeSalesIfExists(string filePath, IReadOnlyCollection<Product> products)
    {
        return File.Exists(filePath) ? FCsvDataStore.ReadAndNormalizeSales(filePath, products) : Array.Empty<SaleRecord>();
    }

    /// <summary>
    /// 在庫履歴CSVが存在する場合に履歴一覧を読み込みます。
    /// </summary>
    public IReadOnlyList<InventoryHistoryRecord> ReadInventoryHistoriesIfExists(string filePath)
    {
        return File.Exists(filePath) ? FCsvDataStore.ReadInventoryHistories(filePath) : Array.Empty<InventoryHistoryRecord>();
    }

    /// <summary>
    /// 在庫履歴一覧をCSVへ書き込みます。
    /// </summary>
    public void WriteInventoryHistories(string filePath, IEnumerable<InventoryHistoryRecord> records)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return;
        }

        FCsvDataStore.WriteInventoryHistories(filePath, records);
    }
}
