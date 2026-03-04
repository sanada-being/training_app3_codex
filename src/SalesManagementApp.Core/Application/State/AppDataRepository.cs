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

    internal AppDataRepository(CsvDataStore vCsvDataStore)
    {
        FCsvDataStore = vCsvDataStore ?? throw new ArgumentNullException(nameof(vCsvDataStore));
    }

    /// <summary>
    /// 商品CSVが存在する場合に商品一覧を読み込みます。
    /// </summary>
    public IReadOnlyList<Product> ReadProductsIfExists(string vFilePath)
    {
        return File.Exists(vFilePath) ? FCsvDataStore.ReadProducts(vFilePath) : Array.Empty<Product>();
    }

    /// <summary>
    /// 在庫CSVが存在する場合に在庫一覧を読み込みます。
    /// </summary>
    public IReadOnlyList<InventoryRecord> ReadInventoriesIfExists(string vFilePath)
    {
        return File.Exists(vFilePath) ? FCsvDataStore.ReadInventories(vFilePath) : Array.Empty<InventoryRecord>();
    }

    /// <summary>
    /// 売上CSVが存在する場合に正規化を行って読み込みます。
    /// </summary>
    public IReadOnlyList<SaleRecord> ReadAndNormalizeSalesIfExists(string vFilePath, IReadOnlyCollection<Product> vProducts)
    {
        return File.Exists(vFilePath) ? FCsvDataStore.ReadAndNormalizeSales(vFilePath, vProducts) : Array.Empty<SaleRecord>();
    }

    /// <summary>
    /// 在庫履歴CSVが存在する場合に履歴一覧を読み込みます。
    /// </summary>
    public IReadOnlyList<InventoryHistoryRecord> ReadInventoryHistoriesIfExists(string vFilePath)
    {
        return File.Exists(vFilePath) ? FCsvDataStore.ReadInventoryHistories(vFilePath) : Array.Empty<InventoryHistoryRecord>();
    }

    /// <summary>
    /// 在庫履歴一覧をCSVへ書き込みます。
    /// </summary>
    public void WriteInventoryHistories(string vFilePath, IEnumerable<InventoryHistoryRecord> vRecords)
    {
        if (string.IsNullOrWhiteSpace(vFilePath))
        {
            return;
        }

        FCsvDataStore.WriteInventoryHistories(vFilePath, vRecords);
    }
}
