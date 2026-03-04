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
        InventoryStockCalculator vStockCalculator,
        InventoryHistoryService vHistoryService)
    {
        FStockCalculator = vStockCalculator;
        FHistoryService = vHistoryService;
    }

    /// <summary>
    /// 在庫一覧を店舗IDと商品IDで並べて返します。
    /// </summary>
    public IReadOnlyList<InventoryRecord> GetAll(IReadOnlyCollection<InventoryRecord> vRecords)
    {
        return vRecords.OrderBy(vR => vR.StoreId).ThenBy(vR => vR.ProductId).ToList();
    }

    /// <summary>
    /// しきい値以下の在庫を要発注対象として返します。
    /// </summary>
    public IReadOnlyList<InventoryRecord> GetReorderTargets(IReadOnlyCollection<InventoryRecord> vRecords, int vThreshold = 5)
    {
        return vRecords.Where(vR => vR.Stock <= vThreshold).OrderBy(vR => vR.StoreId).ThenBy(vR => vR.ProductId).ToList();
    }

    /// <summary>
    /// 店舗IDと商品ID条件で在庫一覧を絞り込みます。
    /// </summary>
    public IReadOnlyList<InventoryRecord> GetFiltered(
        IReadOnlyCollection<InventoryRecord> vRecords,
        string? vStoreIdFilter,
        string? vProductIdFilter)
    {
        var wNormalizedStoreId = NormalizeOptionalFilter(vStoreIdFilter);
        var wNormalizedProductId = NormalizeOptionalFilter(vProductIdFilter);

        var wQuery = vRecords.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(wNormalizedStoreId))
        {
            wQuery = wQuery.Where(vR => HasIgnoreCaseMatch(vR.StoreId, wNormalizedStoreId));
        }

        if (!string.IsNullOrWhiteSpace(wNormalizedProductId))
        {
            wQuery = wQuery.Where(vR => HasIgnoreCaseMatch(vR.ProductId, wNormalizedProductId));
        }

        return wQuery.OrderBy(vR => vR.StoreId).ThenBy(vR => vR.ProductId).ToList();
    }

    /// <summary>
    /// 在庫を入庫数量分だけ加算します。
    /// </summary>
    public void AddStock(ICollection<InventoryRecord> vRecords, string vStoreId, string vProductId, int vQuantity)
    {
        AddStock(vRecords, vStoreId, vProductId, vQuantity, null, default);
    }

    /// <summary>
    /// 在庫を入庫数量分だけ加算します。
    /// </summary>
    public void AddStock(
        ICollection<InventoryRecord> vRecords,
        string vStoreId,
        string vProductId,
        int vQuantity,
        ICollection<InventoryHistoryRecord>? vHistories,
        DateTime vOccurredAt)
    {
        var wNormalizedStoreId = ValidationGuard.RequireNotEmpty(vStoreId, "StoreId");
        var wNormalizedProductId = ValidationGuard.RequireNotEmpty(vProductId, "ProductId");
        ValidationGuard.RequirePositive(vQuantity, "Quantity");

        lock (FSyncRoot)
        {
            var wTarget = FindOrCreate(vRecords, wNormalizedStoreId, wNormalizedProductId);
            wTarget.Stock = FStockCalculator.CalculateAfterInbound(wTarget.Stock, vQuantity);
            RecordHistory(vHistories, vOccurredAt, InventoryOperationTypeEnum.Inbound, wNormalizedStoreId, wNormalizedProductId, vQuantity, wTarget.Stock);
        }
    }

    /// <summary>
    /// 在庫を出庫数量分だけ減算します。
    /// </summary>
    public void RemoveStock(ICollection<InventoryRecord> vRecords, string vStoreId, string vProductId, int vQuantity)
    {
        RemoveStock(vRecords, vStoreId, vProductId, vQuantity, null, default);
    }

    /// <summary>
    /// 在庫を出庫数量分だけ減算します。
    /// </summary>
    public void RemoveStock(
        ICollection<InventoryRecord> vRecords,
        string vStoreId,
        string vProductId,
        int vQuantity,
        ICollection<InventoryHistoryRecord>? vHistories,
        DateTime vOccurredAt)
    {
        var wNormalizedStoreId = ValidationGuard.RequireNotEmpty(vStoreId, "StoreId");
        var wNormalizedProductId = ValidationGuard.RequireNotEmpty(vProductId, "ProductId");
        ValidationGuard.RequirePositive(vQuantity, "Quantity");

        lock (FSyncRoot)
        {
            var wTarget = vRecords.FirstOrDefault(vR => vR.StoreId == wNormalizedStoreId && vR.ProductId == wNormalizedProductId);
            if (wTarget is null)
            {
                throw new DomainValidationException("対象在庫が存在しません。");
            }

            wTarget.Stock = FStockCalculator.CalculateAfterOutbound(wTarget.Stock, vQuantity);
            RecordHistory(vHistories, vOccurredAt, InventoryOperationTypeEnum.Outbound, wNormalizedStoreId, wNormalizedProductId, vQuantity, wTarget.Stock);
        }
    }

    private static InventoryRecord FindOrCreate(ICollection<InventoryRecord> vRecords, string vStoreId, string vProductId)
    {
        var wTarget = vRecords.FirstOrDefault(vR => vR.StoreId == vStoreId && vR.ProductId == vProductId);
        if (wTarget is not null)
        {
            return wTarget;
        }

        wTarget = new InventoryRecord
        {
            StoreId = vStoreId,
            ProductId = vProductId,
            Stock = 0
        };
        vRecords.Add(wTarget);
        return wTarget;
    }

    private void RecordHistory(
        ICollection<InventoryHistoryRecord>? vHistories,
        DateTime vOccurredAt,
        InventoryOperationTypeEnum vOperationType,
        string vStoreId,
        string vProductId,
        int vQuantity,
        int vResultStock)
    {
        if (vHistories is null)
        {
            return;
        }

        var wTimestamp = vOccurredAt == default ? DateTime.Now : vOccurredAt;
        FHistoryService.Record(
            vHistories,
            wTimestamp,
            vOperationType,
            vStoreId,
            vProductId,
            vQuantity,
            vResultStock,
            "Success");
    }

    private static string NormalizeOptionalFilter(string? vValue)
    {
        if (string.IsNullOrWhiteSpace(vValue))
        {
            return string.Empty;
        }

        return vValue!.Trim();
    }

    private static bool HasIgnoreCaseMatch(string vSource, string vKeyword)
    {
        return vSource?.IndexOf(vKeyword, StringComparison.OrdinalIgnoreCase) >= 0;
    }
}

