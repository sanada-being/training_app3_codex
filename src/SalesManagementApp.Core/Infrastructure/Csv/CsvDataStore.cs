using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using SalesManagementApp.Core.Application.Exceptions;
using SalesManagementApp.Core.Domain.Entities;

namespace SalesManagementApp.Core.Infrastructure.Csv;

public class CsvDataStore
{
    public IReadOnlyList<Product> ReadProducts(string filePath)
    {
        var rows = ReadDataRows(filePath, "ProductId,ProductName,UnitPrice,Category");
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

    public IReadOnlyList<InventoryRecord> ReadInventories(string filePath)
    {
        var rows = ReadDataRows(filePath, "StoreId,ProductId,Stock");
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

    public IReadOnlyList<SaleRecord> ReadSales(string filePath)
    {
        var rows = ReadDataRows(filePath, "SaleDate,StoreId,ProductId,Quantity");
        var result = new List<SaleRecord>();

        for (var i = 0; i < rows.Count; i++)
        {
            var lineNo = i + 2;
            var cells = SplitAndValidateColumns(rows[i], 4, lineNo);

            result.Add(new SaleRecord
            {
                SaleDate = ParseDate(cells[0], nameof(SaleRecord.SaleDate), lineNo),
                StoreId = Require(cells[1], nameof(SaleRecord.StoreId), lineNo),
                ProductId = Require(cells[2], nameof(SaleRecord.ProductId), lineNo),
                Quantity = ParseInt(cells[3], nameof(SaleRecord.Quantity), lineNo, min: 1)
            });
        }

        return result;
    }

    public void WriteProducts(string filePath, IEnumerable<Product> products)
    {
        var lines = new List<string> { "ProductId,ProductName,UnitPrice,Category" };
        lines.AddRange(products.Select(p => $"{p.ProductId},{p.ProductName},{p.UnitPrice},{p.Category}"));
        WriteAllLines(filePath, lines);
    }

    public void WriteInventories(string filePath, IEnumerable<InventoryRecord> records)
    {
        var lines = new List<string> { "StoreId,ProductId,Stock" };
        lines.AddRange(records.Select(r => $"{r.StoreId},{r.ProductId},{r.Stock}"));
        WriteAllLines(filePath, lines);
    }

    public void WriteSales(string filePath, IEnumerable<SaleRecord> records)
    {
        var lines = new List<string> { "SaleDate,StoreId,ProductId,Quantity" };
        lines.AddRange(records.Select(r =>
            $"{r.SaleDate:yyyy-MM-dd},{r.StoreId},{r.ProductId},{r.Quantity}"));
        WriteAllLines(filePath, lines);
    }

    private static List<string> ReadDataRows(string filePath, string expectedHeader)
    {
        if (!File.Exists(filePath))
        {
            throw new DomainValidationException($"ファイルが存在しません: {filePath}");
        }

        var lines = File.ReadAllLines(filePath, Encoding.UTF8).ToList();
        if (lines.Count == 0)
        {
            throw new DomainValidationException($"ファイルが空です: {filePath}");
        }

        if (!string.Equals(lines[0].Trim(), expectedHeader, StringComparison.Ordinal))
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

    private static void WriteAllLines(string filePath, IEnumerable<string> lines)
    {
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllLines(filePath, lines, Encoding.UTF8);
    }
}
