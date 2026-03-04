using System;
using System.Collections.Generic;
using System.Linq;
using SalesManagementApp.Core.Application.Validation;
using SalesManagementApp.Core.Domain.Entities;

namespace SalesManagementApp.Core.Application.Services;

/// <summary>
/// 在庫履歴の記録と条件検索を扱うアプリケーションサービスです。
/// </summary>
public class InventoryHistoryService
{
    /// <summary>
    /// 在庫履歴を発生日時の降順で返します。
    /// </summary>
    public IReadOnlyList<InventoryHistoryRecord> GetAll(IReadOnlyCollection<InventoryHistoryRecord> vHistories)
    {
        return vHistories
            .OrderByDescending(vH => vH.OccurredAt)
            .ThenBy(vH => vH.StoreId)
            .ThenBy(vH => vH.ProductId)
            .ToList();
    }

    /// <summary>
    /// 条件に一致する在庫履歴のみを抽出して返します。
    /// </summary>
    public IReadOnlyList<InventoryHistoryRecord> Filter(
        IReadOnlyCollection<InventoryHistoryRecord> vHistories,
        DateTime? vStartDateTime,
        DateTime? vEndDateTime,
        string? vStoreId,
        string? vProductId,
        InventoryOperationTypeEnum? vOperationType)
    {
        if (vStartDateTime.HasValue && vEndDateTime.HasValue)
        {
            if (vStartDateTime.Value > vEndDateTime.Value)
            {
                throw new SalesManagementApp.Core.Application.Exceptions.DomainValidationException(
                    "Start date-time must be less than or equal to end date-time.");
            }
        }

        var wNormalizedStoreId = NormalizeOptionalFilter(vStoreId);
        var wNormalizedProductId = NormalizeOptionalFilter(vProductId);

        var wQuery = vHistories.AsEnumerable();

        if (vStartDateTime.HasValue)
        {
            wQuery = wQuery.Where(vH => vH.OccurredAt >= vStartDateTime.Value);
        }

        if (vEndDateTime.HasValue)
        {
            wQuery = wQuery.Where(vH => vH.OccurredAt <= vEndDateTime.Value);
        }

        if (!string.IsNullOrWhiteSpace(wNormalizedStoreId))
        {
            wQuery = wQuery.Where(vH => vH.StoreId == wNormalizedStoreId);
        }

        if (!string.IsNullOrWhiteSpace(wNormalizedProductId))
        {
            wQuery = wQuery.Where(vH => vH.ProductId == wNormalizedProductId);
        }

        if (vOperationType.HasValue)
        {
            wQuery = wQuery.Where(vH => vH.OperationType == vOperationType.Value);
        }

        return wQuery
            .OrderByDescending(vH => vH.OccurredAt)
            .ThenBy(vH => vH.StoreId)
            .ThenBy(vH => vH.ProductId)
            .ToList();
    }

    /// <summary>
    /// 在庫操作履歴を1件追加します。
    /// </summary>
    public void Record(
        ICollection<InventoryHistoryRecord> vHistories,
        DateTime vOccurredAt,
        InventoryOperationTypeEnum vOperationType,
        string vStoreId,
        string vProductId,
        int vQuantity,
        int vResultStock,
        string vResult)
    {
        if (vHistories is null)
        {
            throw new ArgumentNullException(nameof(vHistories));
        }

        var wNormalizedStoreId = ValidationGuard.RequireNotEmpty(vStoreId, "StoreId");
        var wNormalizedProductId = ValidationGuard.RequireNotEmpty(vProductId, "ProductId");
        var wNormalizedResult = ValidationGuard.RequireNotEmpty(vResult, "Result");
        ValidationGuard.RequirePositive(vQuantity, "Quantity");
        ValidationGuard.RequireNonNegative(vResultStock, "ResultStock");

        vHistories.Add(new InventoryHistoryRecord
        {
            OccurredAt = vOccurredAt,
            OperationType = vOperationType,
            StoreId = wNormalizedStoreId,
            ProductId = wNormalizedProductId,
            Quantity = vQuantity,
            ResultStock = vResultStock,
            Result = wNormalizedResult
        });
    }

    private static string NormalizeOptionalFilter(string? vValue)
    {
        if (string.IsNullOrWhiteSpace(vValue))
        {
            return string.Empty;
        }

        return vValue!.Trim();
    }
}

