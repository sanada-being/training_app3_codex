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
    /// 公開メソッドです。
    /// </summary>
    public IReadOnlyList<InventoryHistoryRecord> GetAll(IReadOnlyCollection<InventoryHistoryRecord> histories)
    {
        return histories
            .OrderByDescending(h => h.OccurredAt)
            .ThenBy(h => h.StoreId)
            .ThenBy(h => h.ProductId)
            .ToList();
    }

    /// <summary>
    /// 公開メソッドです。
    /// </summary>
    public IReadOnlyList<InventoryHistoryRecord> Filter(
        IReadOnlyCollection<InventoryHistoryRecord> histories,
        DateTime? startDateTime,
        DateTime? endDateTime,
        string? storeId,
        string? productId,
        InventoryOperationType? operationType)
    {
        if (startDateTime.HasValue && endDateTime.HasValue)
        {
            if (startDateTime.Value > endDateTime.Value)
            {
                throw new SalesManagementApp.Core.Application.Exceptions.DomainValidationException(
                    "Start date-time must be less than or equal to end date-time.");
            }
        }

        var normalizedStoreId = NormalizeOptionalFilter(storeId);
        var normalizedProductId = NormalizeOptionalFilter(productId);

        var query = histories.AsEnumerable();

        if (startDateTime.HasValue)
        {
            query = query.Where(h => h.OccurredAt >= startDateTime.Value);
        }

        if (endDateTime.HasValue)
        {
            query = query.Where(h => h.OccurredAt <= endDateTime.Value);
        }

        if (!string.IsNullOrWhiteSpace(normalizedStoreId))
        {
            query = query.Where(h => h.StoreId == normalizedStoreId);
        }

        if (!string.IsNullOrWhiteSpace(normalizedProductId))
        {
            query = query.Where(h => h.ProductId == normalizedProductId);
        }

        if (operationType.HasValue)
        {
            query = query.Where(h => h.OperationType == operationType.Value);
        }

        return query
            .OrderByDescending(h => h.OccurredAt)
            .ThenBy(h => h.StoreId)
            .ThenBy(h => h.ProductId)
            .ToList();
    }

    /// <summary>
    /// 公開メソッドです。
    /// </summary>
    public void Record(
        ICollection<InventoryHistoryRecord> histories,
        DateTime occurredAt,
        InventoryOperationType operationType,
        string storeId,
        string productId,
        int quantity,
        int resultStock,
        string result)
    {
        if (histories is null)
        {
            throw new ArgumentNullException(nameof(histories));
        }

        var normalizedStoreId = ValidationGuard.RequireNotEmpty(storeId, "StoreId");
        var normalizedProductId = ValidationGuard.RequireNotEmpty(productId, "ProductId");
        var normalizedResult = ValidationGuard.RequireNotEmpty(result, "Result");
        ValidationGuard.RequirePositive(quantity, "Quantity");
        ValidationGuard.RequireNonNegative(resultStock, "ResultStock");

        histories.Add(new InventoryHistoryRecord
        {
            OccurredAt = occurredAt,
            OperationType = operationType,
            StoreId = normalizedStoreId,
            ProductId = normalizedProductId,
            Quantity = quantity,
            ResultStock = resultStock,
            Result = normalizedResult
        });
    }

    private static string NormalizeOptionalFilter(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        return value!.Trim();
    }
}
