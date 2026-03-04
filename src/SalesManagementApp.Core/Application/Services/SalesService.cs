using System;
using System.Collections.Generic;
using System.Linq;
using SalesManagementApp.Core.Application.Exceptions;
using SalesManagementApp.Core.Application.Validation;
using SalesManagementApp.Core.Domain.Entities;

namespace SalesManagementApp.Core.Application.Services;

/// <summary>
/// 売上登録と在庫引当を扱うアプリケーションサービスです。
/// </summary>
public class SalesService
{
    private readonly InventoryHistoryService FHistoryService;

    /// <summary>
    /// 売上登録時に使用する依存サービスを初期化します。
    /// </summary>
    public SalesService()
        : this(new InventoryHistoryService())
    {
    }

    internal SalesService(InventoryHistoryService vHistoryService)
    {
        FHistoryService = vHistoryService;
    }

    /// <summary>
    /// 売上一覧を日付降順で返します。
    /// </summary>
    public IReadOnlyList<SaleRecord> GetAll(IReadOnlyCollection<SaleRecord> vSales)
    {
        return vSales
            .OrderByDescending(vS => vS.SaleDate)
            .ThenBy(vS => vS.StoreId)
            .ThenBy(vS => vS.ProductId)
            .ToList();
    }

    /// <summary>
    /// 期間・店舗・商品条件で売上一覧を絞り込みます。
    /// </summary>
    public IReadOnlyList<SaleRecord> GetFiltered(
        IReadOnlyCollection<SaleRecord> vSales,
        DateTime? vStartDate,
        DateTime? vEndDate,
        string? vStoreIdFilter,
        string? vProductIdFilter)
    {
        if (vStartDate.HasValue && vEndDate.HasValue && vStartDate.Value.Date > vEndDate.Value.Date)
        {
            throw new DomainValidationException("Start date must be earlier than or equal to end date.");
        }

        var wNormalizedStoreId = NormalizeOptionalFilter(vStoreIdFilter);
        var wNormalizedProductId = NormalizeOptionalFilter(vProductIdFilter);
        var wQuery = vSales.AsEnumerable();

        if (vStartDate.HasValue)
        {
            var wStart = vStartDate.Value.Date;
            wQuery = wQuery.Where(vS => vS.SaleDate.Date >= wStart);
        }

        if (vEndDate.HasValue)
        {
            var wEnd = vEndDate.Value.Date;
            wQuery = wQuery.Where(vS => vS.SaleDate.Date <= wEnd);
        }

        if (!string.IsNullOrWhiteSpace(wNormalizedStoreId))
        {
            wQuery = wQuery.Where(vS => HasIgnoreCaseMatch(vS.StoreId, wNormalizedStoreId));
        }

        if (!string.IsNullOrWhiteSpace(wNormalizedProductId))
        {
            wQuery = wQuery.Where(vS => HasIgnoreCaseMatch(vS.ProductId, wNormalizedProductId));
        }

        return wQuery
            .OrderByDescending(vS => vS.SaleDate)
            .ThenBy(vS => vS.StoreId)
            .ThenBy(vS => vS.ProductId)
            .ToList();
    }

    /// <summary>
    /// 売上を登録し、在庫を減算します。
    /// </summary>
    public SaleRecord RegisterSale(
        ICollection<SaleRecord> vSales,
        IReadOnlyCollection<Product> vProducts,
        ICollection<InventoryRecord> vInventories,
        SaleRecord vInput)
    {
        return RegisterSale(vSales, vProducts, vInventories, vInput, null, default);
    }

    /// <summary>
    /// 売上を登録し、在庫を減算します。
    /// </summary>
    public SaleRecord RegisterSale(
        ICollection<SaleRecord> vSales,
        IReadOnlyCollection<Product> vProducts,
        ICollection<InventoryRecord> vInventories,
        SaleRecord vInput,
        ICollection<InventoryHistoryRecord>? vHistories,
        DateTime vOccurredAt)
    {
        ValidateInput(vInput);

        var wNormalizedStoreId = vInput.StoreId.Trim();
        var wNormalizedProductId = vInput.ProductId.Trim();

        var wProduct = vProducts.FirstOrDefault(vP => vP.ProductId == wNormalizedProductId);
        if (wProduct is null)
        {
            throw new DomainValidationException("商品が存在しません。");
        }

        var wInventory = vInventories.FirstOrDefault(vI =>
            vI.StoreId == wNormalizedStoreId &&
            vI.ProductId == wNormalizedProductId);

        if (wInventory is null || wInventory.Stock < vInput.Quantity)
        {
            throw new DomainValidationException("在庫が不足しています。");
        }

        var wRecord = new SaleRecord
        {
            SaleDate = vInput.SaleDate,
            StoreId = wNormalizedStoreId,
            ProductId = wNormalizedProductId,
            Quantity = vInput.Quantity,
            SalesAmount = checked(wProduct.UnitPrice * vInput.Quantity)
        };

        wInventory.Stock -= vInput.Quantity;
        vSales.Add(wRecord);

        if (vHistories is not null)
        {
            var wTimestamp = vOccurredAt == default ? DateTime.Now : vOccurredAt;
            FHistoryService.Record(
                vHistories,
                wTimestamp,
                InventoryOperationTypeEnum.Sale,
                wNormalizedStoreId,
                wNormalizedProductId,
                vInput.Quantity,
                wInventory.Stock,
                "Success");
        }

        return wRecord;
    }

    private static void ValidateInput(SaleRecord vInput)
    {
        if (vInput.SaleDate == default)
        {
            throw new DomainValidationException("販売日は必須です。");
        }

        ValidationGuard.RequireNotEmpty(vInput.StoreId, "StoreId");
        ValidationGuard.RequireNotEmpty(vInput.ProductId, "ProductId");
        ValidationGuard.RequirePositive(vInput.Quantity, "Quantity");
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

