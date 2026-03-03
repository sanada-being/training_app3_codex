using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SalesManagementApp.Core.Application.Exceptions;
using SalesManagementApp.Core.Domain.Entities;
using SalesManagementApp.Core.Infrastructure.DataProtection;

namespace SalesManagementApp.Core.Infrastructure.Csv;

public class CsvDataStore
{
    private const string SalesHeaderLegacy = "SaleDate,StoreId,ProductId,Quantity";
    private const string SalesHeaderStandard = "SaleDate,StoreId,ProductId,Quantity,SalesAmount";

    private readonly DataProtectionService _dataProtectionService;

    public CsvDataStore()
        : this(new DataProtectionService())
    {
    }

    internal CsvDataStore(DataProtectionService dataProtectionService)
    {
        _dataProtectionService = dataProtectionService;
    }

    public IReadOnlyList<Product> ReadProducts(string filePath)
    {
        var rows = ReadDataRows(filePath, out _, "ProductId,ProductName,UnitPrice,Category");
        var result = new List<Product>();

        for (var i = 0; i < rows.Count; i++)
        {
            var lineNo = i + 2;
            var cells = SplitAndValidateColumns(rows[i], 4, lineNo);

            var productId = Require(cells[0], nameof(Product.ProductId), lineNo);
            var name = Require(cells[1], nameof(Product.ProductName), lineNo);
            var unitPrice = ParseInt(cells[2], nameof(Product.UnitPrice), lineNo, min: 0);
            var category = Require(cells[3], nameof(Product.Category), lineNo);

            result.Add(new Product
            {
                ProductId = productId,
                ProductName = name,
                UnitPrice = unitPrice,
                Category = category
            });
        }

        return result;
    }

    public Task<IReadOnlyList<Product>> ReadProductsAsync(string filePath, CancellationToken cancellationToken = default)
    {
        return Task.Run(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            return ReadProducts(filePath);
        }, cancellationToken);
    }

    public IReadOnlyList<InventoryRecord> ReadInventories(string filePath)
    {
        var rows = ReadDataRows(filePath, out _, "StoreId,ProductId,Stock");
        var result = new List<InventoryRecord>();

        for (var i = 0; i < rows.Count; i++)
        {
            var lineNo = i + 2;
            var cells = SplitAndValidateColumns(rows[i], 3, lineNo);

            result.Add(new InventoryRecord
            {
                StoreId = Require(cells[0], nameof(InventoryRecord.StoreId), lineNo),
                ProductId = Require(cells[1], nameof(InventoryRecord.ProductId), lineNo),
                Stock = ParseInt(cells[2], nameof(InventoryRecord.Stock), lineNo, min: 0)
            });
        }

        return result;
    }

    public Task<IReadOnlyList<InventoryRecord>> ReadInventoriesAsync(string filePath, CancellationToken cancellationToken = default)
    {
        return Task.Run(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            return ReadInventories(filePath);
        }, cancellationToken);
    }

    public IReadOnlyList<SaleRecord> ReadSales(string filePath)
    {
        var rows = ReadDataRows(
            filePath,
            out var header,
            SalesHeaderLegacy,
            SalesHeaderStandard);
        var hasSalesAmount = string.Equals(header, SalesHeaderStandard, StringComparison.Ordinal);
        var result = new List<SaleRecord>();

        for (var i = 0; i < rows.Count; i++)
        {
            var lineNo = i + 2;
            var cells = SplitAndValidateColumns(rows[i], hasSalesAmount ? 5 : 4, lineNo);

            result.Add(new SaleRecord
            {
                SaleDate = ParseDate(cells[0], nameof(SaleRecord.SaleDate), lineNo),
                StoreId = Require(cells[1], nameof(SaleRecord.StoreId), lineNo),
                ProductId = Require(cells[2], nameof(SaleRecord.ProductId), lineNo),
                Quantity = ParseInt(cells[3], nameof(SaleRecord.Quantity), lineNo, min: 1),
                SalesAmount = hasSalesAmount
                    ? ParseInt(cells[4], nameof(SaleRecord.SalesAmount), lineNo, min: 0)
                    : 0
            });
        }

        return result;
    }

    public IReadOnlyList<SaleRecord> ReadAndNormalizeSales(string filePath, IReadOnlyCollection<Product> products)
    {
        if (products is null)
        {
            throw new ArgumentNullException(nameof(products));
        }

        var rows = ReadDataRows(filePath, out var header, SalesHeaderLegacy, SalesHeaderStandard);
        var hasSalesAmount = string.Equals(header, SalesHeaderStandard, StringComparison.Ordinal);
        var unitPriceMap = BuildUnitPriceMap(products);
        var result = new List<SaleRecord>();

        for (var i = 0; i < rows.Count; i++)
        {
            var lineNo = i + 2;
            var cells = SplitAndValidateColumns(rows[i], hasSalesAmount ? 5 : 4, lineNo);
            var saleDate = ParseDate(cells[0], nameof(SaleRecord.SaleDate), lineNo);
            var storeId = Require(cells[1], nameof(SaleRecord.StoreId), lineNo);
            var productId = Require(cells[2], nameof(SaleRecord.ProductId), lineNo);
            var quantity = ParseInt(cells[3], nameof(SaleRecord.Quantity), lineNo, min: 1);
            var expectedAmount = CalculateSalesAmount(unitPriceMap, productId, quantity, lineNo);
            var salesAmount = hasSalesAmount
                ? ParseInt(cells[4], nameof(SaleRecord.SalesAmount), lineNo, min: 0)
                : expectedAmount;

            if (hasSalesAmount && salesAmount != expectedAmount)
            {
                throw new DomainValidationException(
                    $"行{lineNo}: SalesAmount が不正です。期待値={expectedAmount}, 実値={salesAmount}");
            }

            result.Add(new SaleRecord
            {
                SaleDate = saleDate,
                StoreId = storeId,
                ProductId = productId,
                Quantity = quantity,
                SalesAmount = salesAmount
            });
        }

        if (!hasSalesAmount)
        {
            WriteSales(filePath, result);
        }

        return result;
    }

    public Task<IReadOnlyList<SaleRecord>> ReadSalesAsync(string filePath, CancellationToken cancellationToken = default)
    {
        return Task.Run(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            return ReadSales(filePath);
        }, cancellationToken);
    }

    public void WriteProducts(string filePath, IEnumerable<Product> products)
    {
        var lines = new List<string> { "ProductId,ProductName,UnitPrice,Category" };
        lines.AddRange(products.Select(p => $"{p.ProductId},{p.ProductName},{p.UnitPrice},{p.Category}"));
        WriteAllLines(filePath, lines);
    }

    public Task WriteProductsAsync(string filePath, IEnumerable<Product> products, CancellationToken cancellationToken = default)
    {
        return Task.Run(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            WriteProducts(filePath, products);
        }, cancellationToken);
    }

    public void WriteInventories(string filePath, IEnumerable<InventoryRecord> records)
    {
        var lines = new List<string> { "StoreId,ProductId,Stock" };
        lines.AddRange(records.Select(r => $"{r.StoreId},{r.ProductId},{r.Stock}"));
        WriteAllLines(filePath, lines);
    }

    public Task WriteInventoriesAsync(string filePath, IEnumerable<InventoryRecord> records, CancellationToken cancellationToken = default)
    {
        return Task.Run(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            WriteInventories(filePath, records);
        }, cancellationToken);
    }

    public void WriteSales(string filePath, IEnumerable<SaleRecord> records)
    {
        var lines = new List<string> { SalesHeaderStandard };
        lines.AddRange(records.Select(r =>
            $"{r.SaleDate:yyyy-MM-dd},{r.StoreId},{r.ProductId},{r.Quantity},{r.SalesAmount}"));
        WriteAllLines(filePath, lines);
    }

    public Task WriteSalesAsync(string filePath, IEnumerable<SaleRecord> records, CancellationToken cancellationToken = default)
    {
        return Task.Run(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            WriteSales(filePath, records);
        }, cancellationToken);
    }

    public IReadOnlyList<string> GetBackups(string filePath)
    {
        return _dataProtectionService.GetBackupFiles(filePath);
    }

    public void RestoreLatestBackup(string filePath)
    {
        _dataProtectionService.RestoreLatestBackup(filePath);
        _dataProtectionService.WriteLog(filePath, "WARN", $"Restored from backup: {filePath}");
    }

    private static Dictionary<string, int> BuildUnitPriceMap(IReadOnlyCollection<Product> products)
    {
        var duplicate = products
            .GroupBy(p => p.ProductId)
            .FirstOrDefault(g => g.Count() > 1);

        if (duplicate is not null)
        {
            throw new DomainValidationException(
                $"商品マスタに重複したProductIdが存在するため売上金額を算出できません: {duplicate.Key}");
        }

        return products.ToDictionary(p => p.ProductId, p => p.UnitPrice);
    }

    private static int CalculateSalesAmount(
        IReadOnlyDictionary<string, int> unitPriceMap,
        string productId,
        int quantity,
        int lineNo)
    {
        if (!unitPriceMap.TryGetValue(productId, out var unitPrice))
        {
            throw new DomainValidationException(
                $"行{lineNo}: ProductId={productId} の商品マスタが存在しないため売上金額を算出できません。");
        }

        try
        {
            return checked(unitPrice * quantity);
        }
        catch (OverflowException)
        {
            throw new DomainValidationException(
                $"行{lineNo}: 売上金額が計算上限を超えています。");
        }
    }

    private static List<string> ReadDataRows(string filePath, out string header, params string[] expectedHeaders)
    {
        header = string.Empty;

        if (!File.Exists(filePath))
        {
            throw new DomainValidationException($"ファイルが存在しません: {filePath}");
        }

        var lines = File.ReadAllLines(filePath, Encoding.UTF8).ToList();
        if (lines.Count == 0)
        {
            throw new DomainValidationException($"ファイルが空です: {filePath}");
        }

        var normalizedHeader = lines[0].Trim();
        header = normalizedHeader;
        if (!expectedHeaders.Contains(normalizedHeader, StringComparer.Ordinal))
        {
            throw new DomainValidationException($"ヘッダーが不正です: {filePath}");
        }

        return lines.Skip(1).Where(static l => !string.IsNullOrWhiteSpace(l)).ToList();
    }

    private static string[] SplitAndValidateColumns(string line, int expectedCount, int lineNo)
    {
        var cells = line.Split(',');
        if (cells.Length != expectedCount)
        {
            throw new DomainValidationException($"行{lineNo}: 列数が不正です。期待値={expectedCount}, 実値={cells.Length}");
        }

        return cells;
    }

    private static string Require(string value, string fieldName, int lineNo)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainValidationException($"行{lineNo}: {fieldName} は必須です。");
        }

        return value.Trim();
    }

    private static int ParseInt(string value, string fieldName, int lineNo, int min)
    {
        if (!int.TryParse(value, out var parsed))
        {
            throw new DomainValidationException($"行{lineNo}: {fieldName} は整数である必要があります。");
        }

        if (parsed < min)
        {
            throw new DomainValidationException($"行{lineNo}: {fieldName} は {min} 以上である必要があります。");
        }

        return parsed;
    }

    private static DateTime ParseDate(string value, string fieldName, int lineNo)
    {
        if (!DateTime.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
        {
            throw new DomainValidationException($"行{lineNo}: {fieldName} の日付形式は yyyy-MM-dd である必要があります。");
        }

        return parsed;
    }

    private void WriteAllLines(string filePath, IEnumerable<string> lines)
    {
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var backupPath = _dataProtectionService.CreateBackupIfExists(filePath);
        if (!string.IsNullOrWhiteSpace(backupPath))
        {
            _dataProtectionService.WriteLog(filePath, "INFO", $"Backup created: {backupPath}");
        }

        try
        {
            File.WriteAllLines(filePath, lines, Encoding.UTF8);
            _dataProtectionService.WriteLog(filePath, "INFO", $"Write succeeded: {filePath}");
        }
        catch (Exception ex)
        {
            _dataProtectionService.WriteLog(filePath, "ERROR", $"Write failed: {filePath} / {ex.Message}");
            throw;
        }
    }
}
