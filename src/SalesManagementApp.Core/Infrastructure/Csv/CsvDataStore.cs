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
/// CSVファイルの読み書きを担当するデータストアです。
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
    /// CSV読み書きとバックアップ処理を行うデータストアを初期化します。
    /// </summary>
    public CsvDataStore()
        : this(new DataProtectionService())
    {
    }

    internal CsvDataStore(DataProtectionService vDataProtectionService)
    {
        FDataProtectionService = vDataProtectionService;
    }

    /// <summary>
    /// 商品CSVを読み込み、商品一覧を返します。
    /// </summary>
    public IReadOnlyList<Product> ReadProducts(string vFilePath)
    {
        var wRows = ReadDataRows(vFilePath, out _, C_ProductsHeader);
        var wResult = new List<Product>();

        for (var i = 0; i < wRows.Count; i++)
        {
            var wLineNo = i + 2;
            var wCells = SplitAndValidateColumns(wRows[i], 4, wLineNo);

            var wProductId = Require(wCells[0], nameof(Product.ProductId), wLineNo);
            var wName = Require(wCells[1], nameof(Product.ProductName), wLineNo);
            var wUnitPrice = ParseInt(wCells[2], nameof(Product.UnitPrice), wLineNo, vMin: 0);
            var wCategory = Require(wCells[3], nameof(Product.Category), wLineNo);

            wResult.Add(new Product
            {
                ProductId = wProductId,
                ProductName = wName,
                UnitPrice = wUnitPrice,
                Category = wCategory
            });
        }

        return wResult;
    }

    /// <summary>
    /// 商品CSVを非同期で読み込みます。
    /// </summary>
    public Task<IReadOnlyList<Product>> ReadProductsAsync(string vFilePath, CancellationToken vCancellationToken = default)
    {
        return Task.Run(() =>
        {
            vCancellationToken.ThrowIfCancellationRequested();
            return ReadProducts(vFilePath);
        }, vCancellationToken);
    }

    /// <summary>
    /// 在庫CSVを読み込み、在庫一覧を返します。
    /// </summary>
    public IReadOnlyList<InventoryRecord> ReadInventories(string vFilePath)
    {
        var wRows = ReadDataRows(vFilePath, out _, C_InventoryHeader);
        var wResult = new List<InventoryRecord>();

        for (var i = 0; i < wRows.Count; i++)
        {
            var wLineNo = i + 2;
            var wCells = SplitAndValidateColumns(wRows[i], 3, wLineNo);

            wResult.Add(new InventoryRecord
            {
                StoreId = Require(wCells[0], nameof(InventoryRecord.StoreId), wLineNo),
                ProductId = Require(wCells[1], nameof(InventoryRecord.ProductId), wLineNo),
                Stock = ParseInt(wCells[2], nameof(InventoryRecord.Stock), wLineNo, vMin: 0)
            });
        }

        return wResult;
    }

    /// <summary>
    /// 在庫CSVを非同期で読み込みます。
    /// </summary>
    public Task<IReadOnlyList<InventoryRecord>> ReadInventoriesAsync(string vFilePath, CancellationToken vCancellationToken = default)
    {
        return Task.Run(() =>
        {
            vCancellationToken.ThrowIfCancellationRequested();
            return ReadInventories(vFilePath);
        }, vCancellationToken);
    }

    /// <summary>
    /// 在庫履歴CSVを読み込み、履歴一覧を返します。
    /// </summary>
    public IReadOnlyList<InventoryHistoryRecord> ReadInventoryHistories(string vFilePath)
    {
        var wRows = ReadDataRows(vFilePath, out _, C_InventoryHistoryHeader);
        var wResult = new List<InventoryHistoryRecord>();

        for (var i = 0; i < wRows.Count; i++)
        {
            var wLineNo = i + 2;
            var wCells = SplitAndValidateColumns(wRows[i], 7, wLineNo);

            wResult.Add(new InventoryHistoryRecord
            {
                OccurredAt = ParseDateTime(wCells[0], nameof(InventoryHistoryRecord.OccurredAt), wLineNo),
                OperationType = ParseOperationType(wCells[1], wLineNo),
                StoreId = Require(wCells[2], nameof(InventoryHistoryRecord.StoreId), wLineNo),
                ProductId = Require(wCells[3], nameof(InventoryHistoryRecord.ProductId), wLineNo),
                Quantity = ParseInt(wCells[4], nameof(InventoryHistoryRecord.Quantity), wLineNo, vMin: 1),
                ResultStock = ParseInt(wCells[5], nameof(InventoryHistoryRecord.ResultStock), wLineNo, vMin: 0),
                Result = Require(wCells[6], nameof(InventoryHistoryRecord.Result), wLineNo)
            });
        }

        return wResult;
    }

    /// <summary>
    /// 在庫履歴CSVを非同期で読み込みます。
    /// </summary>
    public Task<IReadOnlyList<InventoryHistoryRecord>> ReadInventoryHistoriesAsync(
        string vFilePath,
        CancellationToken vCancellationToken = default)
    {
        return Task.Run(() =>
        {
            vCancellationToken.ThrowIfCancellationRequested();
            return ReadInventoryHistories(vFilePath);
        }, vCancellationToken);
    }

    /// <summary>
    /// 売上CSVを読み込み、売上一覧を返します。
    /// </summary>
    public IReadOnlyList<SaleRecord> ReadSales(string vFilePath)
    {
        var wRows = ReadDataRows(vFilePath, out var wHeader, C_SalesHeaderLegacy, C_SalesHeaderStandard);
        var wHasSalesAmount = string.Equals(wHeader, C_SalesHeaderStandard, StringComparison.Ordinal);
        var wResult = new List<SaleRecord>();

        for (var i = 0; i < wRows.Count; i++)
        {
            var wLineNo = i + 2;
            var wCells = SplitAndValidateColumns(wRows[i], wHasSalesAmount ? 5 : 4, wLineNo);

            wResult.Add(new SaleRecord
            {
                SaleDate = ParseDate(wCells[0], nameof(SaleRecord.SaleDate), wLineNo),
                StoreId = Require(wCells[1], nameof(SaleRecord.StoreId), wLineNo),
                ProductId = Require(wCells[2], nameof(SaleRecord.ProductId), wLineNo),
                Quantity = ParseInt(wCells[3], nameof(SaleRecord.Quantity), wLineNo, vMin: 1),
                SalesAmount = wHasSalesAmount
                    ? ParseInt(wCells[4], nameof(SaleRecord.SalesAmount), wLineNo, vMin: 0)
                    : 0
            });
        }

        return wResult;
    }

    /// <summary>
    /// 売上CSVを検証・正規化して売上一覧を返します。
    /// </summary>
    public IReadOnlyList<SaleRecord> ReadAndNormalizeSales(string vFilePath, IReadOnlyCollection<Product> vProducts)
    {
        if (vProducts is null)
        {
            throw new ArgumentNullException(nameof(vProducts));
        }

        var wRows = ReadDataRows(vFilePath, out var wHeader, C_SalesHeaderLegacy, C_SalesHeaderStandard);
        var wHasSalesAmount = string.Equals(wHeader, C_SalesHeaderStandard, StringComparison.Ordinal);
        var wUnitPriceMap = BuildUnitPriceMap(vProducts);
        var wResult = new List<SaleRecord>();

        for (var i = 0; i < wRows.Count; i++)
        {
            var wLineNo = i + 2;
            var wCells = SplitAndValidateColumns(wRows[i], wHasSalesAmount ? 5 : 4, wLineNo);
            var wSaleDate = ParseDate(wCells[0], nameof(SaleRecord.SaleDate), wLineNo);
            var wStoreId = Require(wCells[1], nameof(SaleRecord.StoreId), wLineNo);
            var wProductId = Require(wCells[2], nameof(SaleRecord.ProductId), wLineNo);
            var wQuantity = ParseInt(wCells[3], nameof(SaleRecord.Quantity), wLineNo, vMin: 1);
            var wExpectedAmount = CalculateSalesAmount(wUnitPriceMap, wProductId, wQuantity, wLineNo);
            var wSalesAmount = wHasSalesAmount
                ? ParseInt(wCells[4], nameof(SaleRecord.SalesAmount), wLineNo, vMin: 0)
                : wExpectedAmount;

            if (wHasSalesAmount && wSalesAmount != wExpectedAmount)
            {
                throw new DomainValidationException(
                    $"Line {wLineNo}: SalesAmount mismatch. expected={wExpectedAmount}, actual={wSalesAmount}");
            }

            wResult.Add(new SaleRecord
            {
                SaleDate = wSaleDate,
                StoreId = wStoreId,
                ProductId = wProductId,
                Quantity = wQuantity,
                SalesAmount = wSalesAmount
            });
        }

        if (!wHasSalesAmount)
        {
            WriteSales(vFilePath, wResult);
        }

        return wResult;
    }

    /// <summary>
    /// 売上CSVを非同期で読み込みます。
    /// </summary>
    public Task<IReadOnlyList<SaleRecord>> ReadSalesAsync(string vFilePath, CancellationToken vCancellationToken = default)
    {
        return Task.Run(() =>
        {
            vCancellationToken.ThrowIfCancellationRequested();
            return ReadSales(vFilePath);
        }, vCancellationToken);
    }

    /// <summary>
    /// 商品一覧を商品CSVへ書き込みます。
    /// </summary>
    public void WriteProducts(string vFilePath, IEnumerable<Product> vProducts)
    {
        var wLines = new List<string> { C_ProductsHeader };
        wLines.AddRange(vProducts.Select(vP => $"{vP.ProductId},{vP.ProductName},{vP.UnitPrice},{vP.Category}"));
        WriteAllLines(vFilePath, wLines);
    }

    /// <summary>
    /// 商品一覧を商品CSVへ非同期で書き込みます。
    /// </summary>
    public Task WriteProductsAsync(string vFilePath, IEnumerable<Product> vProducts, CancellationToken vCancellationToken = default)
    {
        return Task.Run(() =>
        {
            vCancellationToken.ThrowIfCancellationRequested();
            WriteProducts(vFilePath, vProducts);
        }, vCancellationToken);
    }

    /// <summary>
    /// 在庫一覧を在庫CSVへ書き込みます。
    /// </summary>
    public void WriteInventories(string vFilePath, IEnumerable<InventoryRecord> vRecords)
    {
        var wLines = new List<string> { C_InventoryHeader };
        wLines.AddRange(vRecords.Select(vR => $"{vR.StoreId},{vR.ProductId},{vR.Stock}"));
        WriteAllLines(vFilePath, wLines);
    }

    /// <summary>
    /// 在庫一覧を在庫CSVへ非同期で書き込みます。
    /// </summary>
    public Task WriteInventoriesAsync(string vFilePath, IEnumerable<InventoryRecord> vRecords, CancellationToken vCancellationToken = default)
    {
        return Task.Run(() =>
        {
            vCancellationToken.ThrowIfCancellationRequested();
            WriteInventories(vFilePath, vRecords);
        }, vCancellationToken);
    }

    /// <summary>
    /// 在庫履歴一覧を在庫履歴CSVへ書き込みます。
    /// </summary>
    public void WriteInventoryHistories(string vFilePath, IEnumerable<InventoryHistoryRecord> vRecords)
    {
        var wLines = new List<string> { C_InventoryHistoryHeader };
        wLines.AddRange(vRecords.Select(vR =>
            $"{vR.OccurredAt:yyyy-MM-dd HH:mm:ss},{vR.OperationType},{vR.StoreId},{vR.ProductId},{vR.Quantity},{vR.ResultStock},{vR.Result}"));
        WriteAllLines(vFilePath, wLines);
    }

    /// <summary>
    /// 在庫履歴一覧を在庫履歴CSVへ非同期で書き込みます。
    /// </summary>
    public Task WriteInventoryHistoriesAsync(
        string vFilePath,
        IEnumerable<InventoryHistoryRecord> vRecords,
        CancellationToken vCancellationToken = default)
    {
        return Task.Run(() =>
        {
            vCancellationToken.ThrowIfCancellationRequested();
            WriteInventoryHistories(vFilePath, vRecords);
        }, vCancellationToken);
    }

    /// <summary>
    /// 売上一覧を売上CSVへ書き込みます。
    /// </summary>
    public void WriteSales(string vFilePath, IEnumerable<SaleRecord> vRecords)
    {
        var wLines = new List<string> { C_SalesHeaderStandard };
        wLines.AddRange(vRecords.Select(vR =>
            $"{vR.SaleDate:yyyy-MM-dd},{vR.StoreId},{vR.ProductId},{vR.Quantity},{vR.SalesAmount}"));
        WriteAllLines(vFilePath, wLines);
    }

    /// <summary>
    /// 売上一覧を売上CSVへ非同期で書き込みます。
    /// </summary>
    public Task WriteSalesAsync(string vFilePath, IEnumerable<SaleRecord> vRecords, CancellationToken vCancellationToken = default)
    {
        return Task.Run(() =>
        {
            vCancellationToken.ThrowIfCancellationRequested();
            WriteSales(vFilePath, vRecords);
        }, vCancellationToken);
    }

    /// <summary>
    /// 対象ファイルに対応するバックアップ一覧を取得します。
    /// </summary>
    public IReadOnlyList<string> GetBackups(string vFilePath)
    {
        return FDataProtectionService.GetBackupFiles(vFilePath);
    }

    /// <summary>
    /// 対象ファイルを最新バックアップで復元します。
    /// </summary>
    public void RestoreLatestBackup(string vFilePath)
    {
        FDataProtectionService.RestoreLatestBackup(vFilePath);
        FDataProtectionService.WriteLog(vFilePath, "WARN", $"Restored from backup: {vFilePath}");
    }

    private static Dictionary<string, int> BuildUnitPriceMap(IReadOnlyCollection<Product> vProducts)
    {
        var wDuplicate = vProducts
            .GroupBy(vP => vP.ProductId)
            .FirstOrDefault(vG => vG.Count() > 1);

        if (wDuplicate is not null)
        {
            throw new DomainValidationException(
                $"Duplicate ProductId in product master: {wDuplicate.Key}");
        }

        return vProducts.ToDictionary(vP => vP.ProductId, vP => vP.UnitPrice);
    }

    private static int CalculateSalesAmount(
        IReadOnlyDictionary<string, int> vUnitPriceMap,
        string vProductId,
        int vQuantity,
        int vLineNo)
    {
        if (!vUnitPriceMap.TryGetValue(vProductId, out var wUnitPrice))
        {
            throw new DomainValidationException(
                $"Line {vLineNo}: ProductId={vProductId} is not found in product master.");
        }

        try
        {
            return checked(wUnitPrice * vQuantity);
        }
        catch (OverflowException)
        {
            throw new DomainValidationException(
                $"Line {vLineNo}: SalesAmount overflow.");
        }
    }

    private static List<string> ReadDataRows(string vFilePath, out string vHeader, params string[] vExpectedHeaders)
    {
        vHeader = string.Empty;

        if (!File.Exists(vFilePath))
        {
            throw new DomainValidationException($"File not found: {vFilePath}");
        }

        var wLines = File.ReadAllLines(vFilePath, Encoding.UTF8).ToList();
        if (wLines.Count == 0)
        {
            throw new DomainValidationException($"File is empty: {vFilePath}");
        }

        var wNormalizedHeader = wLines[0].Trim();
        vHeader = wNormalizedHeader;
        if (!vExpectedHeaders.Contains(wNormalizedHeader, StringComparer.Ordinal))
        {
            throw new DomainValidationException($"Header is invalid: {vFilePath}");
        }

        return wLines.Skip(1).Where(static vL => !string.IsNullOrWhiteSpace(vL)).ToList();
    }

    private static string[] SplitAndValidateColumns(string vLine, int vExpectedCount, int vLineNo)
    {
        var wCells = vLine.Split(',');
        if (wCells.Length != vExpectedCount)
        {
            throw new DomainValidationException(
                $"Line {vLineNo}: invalid column count. expected={vExpectedCount}, actual={wCells.Length}");
        }

        return wCells;
    }

    private static string Require(string vValue, string vFieldName, int vLineNo)
    {
        if (string.IsNullOrWhiteSpace(vValue))
        {
            throw new DomainValidationException($"Line {vLineNo}: {vFieldName} is required.");
        }

        return vValue.Trim();
    }

    private static int ParseInt(string vValue, string vFieldName, int vLineNo, int vMin)
    {
        if (!int.TryParse(vValue, out var wParsed))
        {
            throw new DomainValidationException($"Line {vLineNo}: {vFieldName} must be integer.");
        }

        if (wParsed < vMin)
        {
            throw new DomainValidationException($"Line {vLineNo}: {vFieldName} must be >= {vMin}.");
        }

        return wParsed;
    }

    private static DateTime ParseDate(string vValue, string vFieldName, int vLineNo)
    {
        if (!DateTime.TryParseExact(vValue, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var wParsed))
        {
            throw new DomainValidationException(
                $"Line {vLineNo}: {vFieldName} must follow yyyy-MM-dd.");
        }

        return wParsed;
    }

    private static DateTime ParseDateTime(string vValue, string vFieldName, int vLineNo)
    {
        if (!DateTime.TryParseExact(vValue, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var wParsed))
        {
            throw new DomainValidationException(
                $"Line {vLineNo}: {vFieldName} must follow yyyy-MM-dd HH:mm:ss.");
        }

        return wParsed;
    }

    private static InventoryOperationTypeEnum ParseOperationType(string vValue, int vLineNo)
    {
        if (!Enum.TryParse(vValue, ignoreCase: true, out InventoryOperationTypeEnum wParsed)
            || !Enum.IsDefined(typeof(InventoryOperationTypeEnum), wParsed))
        {
            throw new DomainValidationException($"Line {vLineNo}: OperationType is invalid.");
        }

        return wParsed;
    }

    private void WriteAllLines(string vFilePath, IEnumerable<string> vLines)
    {
        var wDirectory = Path.GetDirectoryName(vFilePath);
        if (!string.IsNullOrWhiteSpace(wDirectory))
        {
            Directory.CreateDirectory(wDirectory);
        }

        var wBackupPath = FDataProtectionService.CreateBackupIfExists(vFilePath);
        if (!string.IsNullOrWhiteSpace(wBackupPath))
        {
            FDataProtectionService.WriteLog(vFilePath, "INFO", $"Backup created: {wBackupPath}");
        }

        try
        {
            File.WriteAllLines(vFilePath, vLines, Encoding.UTF8);
            FDataProtectionService.WriteLog(vFilePath, "INFO", $"Write succeeded: {vFilePath}");
        }
        catch (Exception wEx)
        {
            FDataProtectionService.WriteLog(vFilePath, "ERROR", $"Write failed: {vFilePath} / {wEx.Message}");
            throw;
        }
    }
}

