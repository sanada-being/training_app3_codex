using System;
using System.Collections.Generic;
using System.Linq;
using SalesManagementApp.Core.Application.Exceptions;
using SalesManagementApp.Core.Application.Validation;
using SalesManagementApp.Core.Domain.Entities;

namespace SalesManagementApp.Core.Application.Services;

public class SalesService
{
    public IReadOnlyList<SaleRecord> GetAll(IReadOnlyCollection<SaleRecord> sales)
    {
        return sales
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
        ValidateInput(input);

        var normalizedStoreId = input.StoreId.Trim();
        var normalizedProductId = input.ProductId.Trim();

        var product = products.FirstOrDefault(p => p.ProductId == normalizedProductId);
        if (product is null)
        {
            throw new DomainValidationException("存在しない商品です。");
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
        ValidationGuard.RequirePositive(input.Quantity, "数量");
    }
}
