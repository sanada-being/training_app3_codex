using System;
using System.Collections.Generic;
using System.IO;
using SalesManagementApp.Core.Domain.Entities;
using SalesManagementApp.Core.Infrastructure.Csv;

namespace SalesManagementApp.Core.Application.State;

public class AppDataRepository
{
    private readonly CsvDataStore _csvDataStore;

    public AppDataRepository()
        : this(new CsvDataStore())
    {
    }

    internal AppDataRepository(CsvDataStore csvDataStore)
    {
        _csvDataStore = csvDataStore ?? throw new ArgumentNullException(nameof(csvDataStore));
    }

    public IReadOnlyList<Product> ReadProductsIfExists(string filePath)
    {
        return File.Exists(filePath) ? _csvDataStore.ReadProducts(filePath) : Array.Empty<Product>();
    }

    public IReadOnlyList<InventoryRecord> ReadInventoriesIfExists(string filePath)
    {
        return File.Exists(filePath) ? _csvDataStore.ReadInventories(filePath) : Array.Empty<InventoryRecord>();
    }

    public IReadOnlyList<SaleRecord> ReadAndNormalizeSalesIfExists(string filePath, IReadOnlyCollection<Product> products)
    {
        return File.Exists(filePath) ? _csvDataStore.ReadAndNormalizeSales(filePath, products) : Array.Empty<SaleRecord>();
    }

    public IReadOnlyList<InventoryHistoryRecord> ReadInventoryHistoriesIfExists(string filePath)
    {
        return File.Exists(filePath) ? _csvDataStore.ReadInventoryHistories(filePath) : Array.Empty<InventoryHistoryRecord>();
    }

    public void WriteInventoryHistories(string filePath, IEnumerable<InventoryHistoryRecord> records)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return;
        }

        _csvDataStore.WriteInventoryHistories(filePath, records);
    }
}
