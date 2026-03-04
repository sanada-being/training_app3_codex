using System;
using System.Collections.Generic;
using System.Linq;
using SalesManagementApp.Core.Application.Exceptions;
using SalesManagementApp.Core.Application.Validation;
using SalesManagementApp.Core.Domain.Entities;

namespace SalesManagementApp.Core.Application.Services;

public class SalesService
{
    private readonly InventoryHistoryService _historyService;

    public SalesService()
        : this(new InventoryHistoryService())
    {
    }

    internal SalesService(InventoryHistoryService historyService)
    {
        _historyService = historyService;
    }

    public IReadOnlyList<SaleRecord> GetAll(IReadOnlyCollection<SaleRecord> sales)
    {
        return sales
            .OrderByDescending(s => s.SaleDate)
            .ThenBy(s => s.StoreId)
            .ThenBy(s => s.ProductId)
            .ToList();
    }

    public IReadOnlyList<SaleRecord> GetFiltered(
        IReadOnlyCollection<SaleRecord> sales,
        DateTime? startDate,
        DateTime? endDate,
        string? storeIdFilter,
        string? productIdFilter)
    {
        if (startDate.HasValue && endDate.HasValue && startDate.Value.Date > endDate.Value.Date)
        {
            throw new DomainValidationException("Start date must be earlier than or equal to end date.");
        }

        var normalizedStoreId = NormalizeOptionalFilter(storeIdFilter);
        var normalizedProductId = NormalizeOptionalFilter(productIdFilter);
        var query = sales.AsEnumerable();

        if (startDate.HasValue)
        {
            var start = startDate.Value.Date;
            query = query.Where(s => s.SaleDate.Date >= start);
        }

        if (endDate.HasValue)
        {
            var end = endDate.Value.Date;
            query = query.Where(s => s.SaleDate.Date <= end);
        }

        if (!string.IsNullOrWhiteSpace(normalizedStoreId))
        {
            query = query.Where(s => HasIgnoreCaseMatch(s.StoreId, normalizedStoreId));
        }

        if (!string.IsNullOrWhiteSpace(normalizedProductId))
        {
            query = query.Where(s => HasIgnoreCaseMatch(s.ProductId, normalizedProductId));
        }

        return query
            .OrderByDescending(s => s.SaleDate)
            .ThenBy(s => s.StoreId)
            .ThenBy(s => s.ProductId)
            .ToList();
    }

    public SaleRecord RegisterSale(
        ICollection<SaleRecord> sales,
        IReadOnlyCollection<Product> products,
        ICollection<InventoryRecord> inventories,
        SaleRecord input)
    {
        return RegisterSale(sales, products, inventories, input, null, default);
    }

    public SaleRecord RegisterSale(
        ICollection<SaleRecord> sales,
        IReadOnlyCollection<Product> products,
        ICollection<InventoryRecord> inventories,
        SaleRecord input,
        ICollection<InventoryHistoryRecord>? histories,
        DateTime occurredAt)
    {
        ValidateInput(input);

        var normalizedStoreId = input.StoreId.Trim();
        var normalizedProductId = input.ProductId.Trim();

        var product = products.FirstOrDefault(p => p.ProductId == normalizedProductId);
        if (product is null)
        {
            throw new DomainValidationException("商品が存在しません。");
        }

        var inventory = inventories.FirstOrDefault(i =>
            i.StoreId == normalizedStoreId &&
            i.ProductId == normalizedProductId);

        if (inventory is null || inventory.Stock < input.Quantity)
        {
            throw new DomainValidationException("在庫が不足しています。");
        }

        var record = new SaleRecord
        {
            SaleDate = input.SaleDate,
            StoreId = normalizedStoreId,
            ProductId = normalizedProductId,
            Quantity = input.Quantity,
            SalesAmount = checked(product.UnitPrice * input.Quantity)
        };

        inventory.Stock -= input.Quantity;
        sales.Add(record);

        if (histories is not null)
        {
            var timestamp = occurredAt == default ? DateTime.Now : occurredAt;
            _historyService.Record(
                histories,
                timestamp,
                InventoryOperationType.Sale,
                normalizedStoreId,
                normalizedProductId,
                input.Quantity,
                inventory.Stock,
                "Success");
        }

        return record;
    }

    private static void ValidateInput(SaleRecord input)
    {
        if (input.SaleDate == default)
        {
            throw new DomainValidationException("販売日は必須です。");
        }

        ValidationGuard.RequireNotEmpty(input.StoreId, "StoreId");
        ValidationGuard.RequireNotEmpty(input.ProductId, "ProductId");
        ValidationGuard.RequirePositive(input.Quantity, "Quantity");
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
