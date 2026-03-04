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

/// <summary>
/// CsvDataStore クラスです。
/// </summary>
public class CsvDataStore
{
    private const string C_ProductsHeader = "ProductId,ProductName,UnitPrice,Category";
    private const string C_InventoryHeader = "StoreId,ProductId,Stock";
    private const string C_SalesHeaderLegacy = "SaleDate,StoreId,ProductId,Quantity";
    private const string C_SalesHeaderStandard = "SaleDate,StoreId,ProductId,Quantity,SalesAmount";
    private const string C_InventoryHistoryHeader = "OccurredAt,OperationType,StoreId,ProductId,Quantity,ResultStock,Result";

    private readonly DataProtectionService FDataProtectionService;

    /// <summary>
    /// 公開メソッドです。
    /// </summary>
    public CsvDataStore()
        : this(new DataProtectionService())
    {
    }

    internal CsvDataStore(DataProtectionService dataProtectionService)
    {
        FDataProtectionService = dataProtectionService;
    }

    /// <summary>
    /// 公開メソッドです。
    /// </summary>
    public IReadOnlyList<Product> ReadProducts(string filePath)
    {
        var rows = ReadDataRows(filePath, out _, C_ProductsHeader);
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

    /// <summary>
    /// 公開メソッドです。
    /// </summary>
    public Task<IReadOnlyList<Product>> ReadProductsAsync(string filePath, CancellationToken cancellationToken = default)
    {
        return Task.Run(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            return ReadProducts(filePath);
        }, cancellationToken);
    }

    /// <summary>
    /// 公開メソッドです。
    /// </summary>
    public IReadOnlyList<InventoryRecord> ReadInventories(string filePath)
    {
        var rows = ReadDataRows(filePath, out _, C_InventoryHeader);
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

    /// <summary>
    /// 公開メソッドです。
    /// </summary>
    public Task<IReadOnlyList<InventoryRecord>> ReadInventoriesAsync(string filePath, CancellationToken cancellationToken = default)
    {
        return Task.Run(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            return ReadInventories(filePath);
        }, cancellationToken);
    }

    /// <summary>
    /// 公開メソッドです。
    /// </summary>
    public IReadOnlyList<InventoryHistoryRecord> ReadInventoryHistories(string filePath)
    {
        var rows = ReadDataRows(filePath, out _, C_InventoryHistoryHeader);
        var result = new List<InventoryHistoryRecord>();

        for (var i = 0; i < rows.Count; i++)
        {
            var lineNo = i + 2;
            var cells = SplitAndValidateColumns(rows[i], 7, lineNo);

            result.Add(new InventoryHistoryRecord
            {
                OccurredAt = ParseDateTime(cells[0], nameof(InventoryHistoryRecord.OccurredAt), lineNo),
                OperationType = ParseOperationType(cells[1], lineNo),
                StoreId = Require(cells[2], nameof(InventoryHistoryRecord.StoreId), lineNo),
                ProductId = Require(cells[3], nameof(InventoryHistoryRecord.ProductId), lineNo),
                Quantity = ParseInt(cells[4], nameof(InventoryHistoryRecord.Quantity), lineNo, min: 1),
                ResultStock = ParseInt(cells[5], nameof(InventoryHistoryRecord.ResultStock), lineNo, min: 0),
                Result = Require(cells[6], nameof(InventoryHistoryRecord.Result), lineNo)
            });
        }

        return result;
    }

    /// <summary>
    /// 公開メソッドです。
    /// </summary>
    public Task<IReadOnlyList<InventoryHistoryRecord>> ReadInventoryHistoriesAsync(
        string filePath,
        CancellationToken cancellationToken = default)
    {
        return Task.Run(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            return ReadInventoryHistories(filePath);
        }, cancellationToken);
    }

    /// <summary>
    /// 公開メソッドです。
    /// </summary>
    public IReadOnlyList<SaleRecord> ReadSales(string filePath)
    {
        var rows = ReadDataRows(filePath, out var header, C_SalesHeaderLegacy, C_SalesHeaderStandard);
        var hasSalesAmount = string.Equals(header, C_SalesHeaderStandard, StringComparison.Ordinal);
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

    /// <summary>
    /// 公開メソッドです。
    /// </summary>
    public IReadOnlyList<SaleRecord> ReadAndNormalizeSales(string filePath, IReadOnlyCollection<Product> products)
    {
        if (products is null)
        {
            throw new ArgumentNullException(nameof(products));
        }

        var rows = ReadDataRows(filePath, out var header, C_SalesHeaderLegacy, C_SalesHeaderStandard);
        var hasSalesAmount = string.Equals(header, C_SalesHeaderStandard, StringComparison.Ordinal);
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
                    $"Line {lineNo}: SalesAmount mismatch. expected={expectedAmount}, actual={salesAmount}");
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

    /// <summary>
    /// 公開メソッドです。
    /// </summary>
    public Task<IReadOnlyList<SaleRecord>> ReadSalesAsync(string filePath, CancellationToken cancellationToken = default)
    {
        return Task.Run(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            return ReadSales(filePath);
        }, cancellationToken);
    }

    /// <summary>
    /// 公開メソッドです。
    /// </summary>
    public void WriteProducts(string filePath, IEnumerable<Product> products)
    {
        var lines = new List<string> { C_ProductsHeader };
        lines.AddRange(products.Select(p => $"{p.ProductId},{p.ProductName},{p.UnitPrice},{p.Category}"));
        WriteAllLines(filePath, lines);
    }

    /// <summary>
    /// 公開メソッドです。
    /// </summary>
    public Task WriteProductsAsync(string filePath, IEnumerable<Product> products, CancellationToken cancellationToken = default)
    {
        return Task.Run(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            WriteProducts(filePath, products);
        }, cancellationToken);
    }

    /// <summary>
    /// 公開メソッドです。
    /// </summary>
    public void WriteInventories(string filePath, IEnumerable<InventoryRecord> records)
    {
        var lines = new List<string> { C_InventoryHeader };
        lines.AddRange(records.Select(r => $"{r.StoreId},{r.ProductId},{r.Stock}"));
        WriteAllLines(filePath, lines);
    }

    /// <summary>
    /// 公開メソッドです。
    /// </summary>
    public Task WriteInventoriesAsync(string filePath, IEnumerable<InventoryRecord> records, CancellationToken cancellationToken = default)
    {
        return Task.Run(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            WriteInventories(filePath, records);
        }, cancellationToken);
    }

    /// <summary>
    /// 公開メソッドです。
    /// </summary>
    public void WriteInventoryHistories(string filePath, IEnumerable<InventoryHistoryRecord> records)
    {
        var lines = new List<string> { C_InventoryHistoryHeader };
        lines.AddRange(records.Select(r =>
            $"{r.OccurredAt:yyyy-MM-dd HH:mm:ss},{r.OperationType},{r.StoreId},{r.ProductId},{r.Quantity},{r.ResultStock},{r.Result}"));
        WriteAllLines(filePath, lines);
    }

    /// <summary>
    /// 公開メソッドです。
    /// </summary>
    public Task WriteInventoryHistoriesAsync(
        string filePath,
        IEnumerable<InventoryHistoryRecord> records,
        CancellationToken cancellationToken = default)
    {
        return Task.Run(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            WriteInventoryHistories(filePath, records);
        }, cancellationToken);
    }

    /// <summary>
    /// 公開メソッドです。
    /// </summary>
    public void WriteSales(string filePath, IEnumerable<SaleRecord> records)
    {
        var lines = new List<string> { C_SalesHeaderStandard };
        lines.AddRange(records.Select(r =>
            $"{r.SaleDate:yyyy-MM-dd},{r.StoreId},{r.ProductId},{r.Quantity},{r.SalesAmount}"));
        WriteAllLines(filePath, lines);
    }

    /// <summary>
    /// 公開メソッドです。
    /// </summary>
    public Task WriteSalesAsync(string filePath, IEnumerable<SaleRecord> records, CancellationToken cancellationToken = default)
    {
        return Task.Run(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            WriteSales(filePath, records);
        }, cancellationToken);
    }

    /// <summary>
    /// 公開メソッドです。
    /// </summary>
    public IReadOnlyList<string> GetBackups(string filePath)
    {
        return FDataProtectionService.GetBackupFiles(filePath);
    }

    /// <summary>
    /// 公開メソッドです。
    /// </summary>
    public void RestoreLatestBackup(string filePath)
    {
        FDataProtectionService.RestoreLatestBackup(filePath);
        FDataProtectionService.WriteLog(filePath, "WARN", $"Restored from backup: {filePath}");
    }

    private static Dictionary<string, int> BuildUnitPriceMap(IReadOnlyCollection<Product> products)
    {
        var duplicate = products
            .GroupBy(p => p.ProductId)
            .FirstOrDefault(g => g.Count() > 1);

        if (duplicate is not null)
        {
            throw new DomainValidationException(
                $"Duplicate ProductId in product master: {duplicate.Key}");
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
                $"Line {lineNo}: ProductId={productId} is not found in product master.");
        }

        try
        {
            return checked(unitPrice * quantity);
        }
        catch (OverflowException)
        {
            throw new DomainValidationException(
                $"Line {lineNo}: SalesAmount overflow.");
        }
    }

    private static List<string> ReadDataRows(string filePath, out string header, params string[] expectedHeaders)
    {
        header = string.Empty;

        if (!File.Exists(filePath))
        {
            throw new DomainValidationException($"File not found: {filePath}");
        }

        var lines = File.ReadAllLines(filePath, Encoding.UTF8).ToList();
        if (lines.Count == 0)
        {
            throw new DomainValidationException($"File is empty: {filePath}");
        }

        var normalizedHeader = lines[0].Trim();
        header = normalizedHeader;
        if (!expectedHeaders.Contains(normalizedHeader, StringComparer.Ordinal))
        {
            throw new DomainValidationException($"Header is invalid: {filePath}");
        }

        return lines.Skip(1).Where(static l => !string.IsNullOrWhiteSpace(l)).ToList();
    }

    private static string[] SplitAndValidateColumns(string line, int expectedCount, int lineNo)
    {
        var cells = line.Split(',');
        if (cells.Length != expectedCount)
        {
            throw new DomainValidationException(
                $"Line {lineNo}: invalid column count. expected={expectedCount}, actual={cells.Length}");
        }

        return cells;
    }

    private static string Require(string value, string fieldName, int lineNo)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainValidationException($"Line {lineNo}: {fieldName} is required.");
        }

        return value.Trim();
    }

    private static int ParseInt(string value, string fieldName, int lineNo, int min)
    {
        if (!int.TryParse(value, out var parsed))
        {
            throw new DomainValidationException($"Line {lineNo}: {fieldName} must be integer.");
        }

        if (parsed < min)
        {
            throw new DomainValidationException($"Line {lineNo}: {fieldName} must be >= {min}.");
        }

        return parsed;
    }

    private static DateTime ParseDate(string value, string fieldName, int lineNo)
    {
        if (!DateTime.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
        {
            throw new DomainValidationException(
                $"Line {lineNo}: {fieldName} must follow yyyy-MM-dd.");
        }

        return parsed;
    }

    private static DateTime ParseDateTime(string value, string fieldName, int lineNo)
    {
        if (!DateTime.TryParseExact(value, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
        {
            throw new DomainValidationException(
                $"Line {lineNo}: {fieldName} must follow yyyy-MM-dd HH:mm:ss.");
        }

        return parsed;
    }

    private static InventoryOperationType ParseOperationType(string value, int lineNo)
    {
        if (!Enum.TryParse(value, ignoreCase: true, out InventoryOperationType parsed)
            || !Enum.IsDefined(typeof(InventoryOperationType), parsed))
        {
            throw new DomainValidationException($"Line {lineNo}: OperationType is invalid.");
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

        var backupPath = FDataProtectionService.CreateBackupIfExists(filePath);
        if (!string.IsNullOrWhiteSpace(backupPath))
        {
            FDataProtectionService.WriteLog(filePath, "INFO", $"Backup created: {backupPath}");
        }

        try
        {
            File.WriteAllLines(filePath, lines, Encoding.UTF8);
            FDataProtectionService.WriteLog(filePath, "INFO", $"Write succeeded: {filePath}");
        }
        catch (Exception ex)
        {
            FDataProtectionService.WriteLog(filePath, "ERROR", $"Write failed: {filePath} / {ex.Message}");
            throw;
        }
    }
}
