using System;
using System.Collections.Generic;
using System.IO;
using SalesManagementApp.Core.Domain.Entities;
using SalesManagementApp.Core.Infrastructure.Csv;

namespace SalesManagementApp.Core.Application.State;

/// <summary>
/// AppDataRepository クラスです。
/// </summary>
public class AppDataRepository
{
    private readonly CsvDataStore FCsvDataStore;

    /// <summary>
    /// 公開メソッドです。
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
    /// 公開メソッドです。
    /// </summary>
    public IReadOnlyList<Product> ReadProductsIfExists(string filePath)
    {
        return File.Exists(filePath) ? FCsvDataStore.ReadProducts(filePath) : Array.Empty<Product>();
    }

    /// <summary>
    /// 公開メソッドです。
    /// </summary>
    public IReadOnlyList<InventoryRecord> ReadInventoriesIfExists(string filePath)
    {
        return File.Exists(filePath) ? FCsvDataStore.ReadInventories(filePath) : Array.Empty<InventoryRecord>();
    }

    /// <summary>
    /// 公開メソッドです。
    /// </summary>
    public IReadOnlyList<SaleRecord> ReadAndNormalizeSalesIfExists(string filePath, IReadOnlyCollection<Product> products)
    {
        return File.Exists(filePath) ? FCsvDataStore.ReadAndNormalizeSales(filePath, products) : Array.Empty<SaleRecord>();
    }

    /// <summary>
    /// 公開メソッドです。
    /// </summary>
    public IReadOnlyList<InventoryHistoryRecord> ReadInventoryHistoriesIfExists(string filePath)
    {
        return File.Exists(filePath) ? FCsvDataStore.ReadInventoryHistories(filePath) : Array.Empty<InventoryHistoryRecord>();
    }

    /// <summary>
    /// 公開メソッドです。
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
