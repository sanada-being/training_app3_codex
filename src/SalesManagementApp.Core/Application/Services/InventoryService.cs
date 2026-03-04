using System;
using System.Collections.Generic;
using System.Linq;
using SalesManagementApp.Core.Application.Exceptions;
using SalesManagementApp.Core.Application.Validation;
using SalesManagementApp.Core.Domain.Entities;

namespace SalesManagementApp.Core.Application.Services;

/// <summary>
/// 在庫の検索と入出庫処理を扱うアプリケーションサービスです。
/// </summary>
public class InventoryService
{
    private readonly object FSyncRoot = new();
    private readonly InventoryStockCalculator FStockCalculator;
    private readonly InventoryHistoryService FHistoryService;

    /// <summary>
    /// 在庫計算と履歴記録の依存関係を初期化します。
    /// </summary>
    public InventoryService()
        : this(new InventoryStockCalculator(), new InventoryHistoryService())
    {
    }

    internal InventoryService(
        InventoryStockCalculator stockCalculator,
        InventoryHistoryService historyService)
    {
        FStockCalculator = stockCalculator;
        FHistoryService = historyService;
    }

    /// <summary>
    /// 在庫一覧を店舗IDと商品IDで並べて返します。
    /// </summary>
    public IReadOnlyList<InventoryRecord> GetAll(IReadOnlyCollection<InventoryRecord> records)
    {
        return records.OrderBy(r => r.StoreId).ThenBy(r => r.ProductId).ToList();
    }

    /// <summary>
    /// しきい値以下の在庫を要発注対象として返します。
    /// </summary>
    public IReadOnlyList<InventoryRecord> GetReorderTargets(IReadOnlyCollection<InventoryRecord> records, int threshold = 5)
    {
        return records.Where(r => r.Stock <= threshold).OrderBy(r => r.StoreId).ThenBy(r => r.ProductId).ToList();
    }

    /// <summary>
    /// 店舗IDと商品ID条件で在庫一覧を絞り込みます。
    /// </summary>
    public IReadOnlyList<InventoryRecord> GetFiltered(
        IReadOnlyCollection<InventoryRecord> records,
        string? storeIdFilter,
        string? productIdFilter)
    {
        var normalizedStoreId = NormalizeOptionalFilter(storeIdFilter);
        var normalizedProductId = NormalizeOptionalFilter(productIdFilter);

        var query = records.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(normalizedStoreId))
        {
            query = query.Where(r => HasIgnoreCaseMatch(r.StoreId, normalizedStoreId));
        }

        if (!string.IsNullOrWhiteSpace(normalizedProductId))
        {
            query = query.Where(r => HasIgnoreCaseMatch(r.ProductId, normalizedProductId));
        }

        return query.OrderBy(r => r.StoreId).ThenBy(r => r.ProductId).ToList();
    }

    /// <summary>
    /// 在庫を入庫数量分だけ加算します。
    /// </summary>
    public void AddStock(ICollection<InventoryRecord> records, string storeId, string productId, int quantity)
    {
        AddStock(records, storeId, productId, quantity, null, default);
    }

    /// <summary>
    /// 在庫を入庫数量分だけ加算します。
    /// </summary>
    public void AddStock(
        ICollection<InventoryRecord> records,
        string storeId,
        string productId,
        int quantity,
        ICollection<InventoryHistoryRecord>? histories,
        DateTime occurredAt)
    {
        var normalizedStoreId = ValidationGuard.RequireNotEmpty(storeId, "StoreId");
        var normalizedProductId = ValidationGuard.RequireNotEmpty(productId, "ProductId");
        ValidationGuard.RequirePositive(quantity, "Quantity");

        lock (FSyncRoot)
        {
            var target = FindOrCreate(records, normalizedStoreId, normalizedProductId);
            target.Stock = FStockCalculator.CalculateAfterInbound(target.Stock, quantity);
            RecordHistory(histories, occurredAt, InventoryOperationType.Inbound, normalizedStoreId, normalizedProductId, quantity, target.Stock);
        }
    }

    /// <summary>
    /// 在庫を出庫数量分だけ減算します。
    /// </summary>
    public void RemoveStock(ICollection<InventoryRecord> records, string storeId, string productId, int quantity)
    {
        RemoveStock(records, storeId, productId, quantity, null, default);
    }

    /// <summary>
    /// 在庫を出庫数量分だけ減算します。
    /// </summary>
    public void RemoveStock(
        ICollection<InventoryRecord> records,
        string storeId,
        string productId,
        int quantity,
        ICollection<InventoryHistoryRecord>? histories,
        DateTime occurredAt)
    {
        var normalizedStoreId = ValidationGuard.RequireNotEmpty(storeId, "StoreId");
        var normalizedProductId = ValidationGuard.RequireNotEmpty(productId, "ProductId");
        ValidationGuard.RequirePositive(quantity, "Quantity");

        lock (FSyncRoot)
        {
            var target = records.FirstOrDefault(r => r.StoreId == normalizedStoreId && r.ProductId == normalizedProductId);
            if (target is null)
            {
                throw new DomainValidationException("対象在庫が存在しません。");
            }

            target.Stock = FStockCalculator.CalculateAfterOutbound(target.Stock, quantity);
            RecordHistory(histories, occurredAt, InventoryOperationType.Outbound, normalizedStoreId, normalizedProductId, quantity, target.Stock);
        }
    }

    private static InventoryRecord FindOrCreate(ICollection<InventoryRecord> records, string storeId, string productId)
    {
        var target = records.FirstOrDefault(r => r.StoreId == storeId && r.ProductId == productId);
        if (target is not null)
        {
            return target;
        }

        target = new InventoryRecord
        {
            StoreId = storeId,
            ProductId = productId,
            Stock = 0
        };
        records.Add(target);
        return target;
    }

    private void RecordHistory(
        ICollection<InventoryHistoryRecord>? histories,
        DateTime occurredAt,
        InventoryOperationType operationType,
        string storeId,
        string productId,
        int quantity,
        int resultStock)
    {
        if (histories is null)
        {
            return;
        }

        var timestamp = occurredAt == default ? DateTime.Now : occurredAt;
        FHistoryService.Record(
            histories,
            timestamp,
            operationType,
            storeId,
            productId,
            quantity,
            resultStock,
            "Success");
    }

    private static string NormalizeOptionalFilter(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        return value!.Trim();
    }

    private static bool HasIgnoreCaseMatch(string source, string keyword)
    {
        return source?.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0;
    }
}
