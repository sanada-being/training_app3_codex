using System;
using System.Collections.Generic;
using System.Linq;
using SalesManagementApp.Core.Application.Exceptions;
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

        var product = products.FirstOrDefault(p => p.ProductId == input.ProductId.Trim());
        if (product is null)
        {
            throw new DomainValidationException("存在しない商品です。");
        }

        var inventory = inventories.FirstOrDefault(i =>
            i.StoreId == input.StoreId.Trim() &&
            i.ProductId == input.ProductId.Trim());

        if (inventory is null || inventory.Stock < input.Quantity)
        {
            throw new DomainValidationException("在庫が不足しています。");
        }

        var record = new SaleRecord
        {
            SaleDate = input.SaleDate,
            StoreId = input.StoreId.Trim(),
            ProductId = input.ProductId.Trim(),
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

        if (string.IsNullOrWhiteSpace(input.StoreId))
        {
            throw new DomainValidationException("StoreId は必須です。");
        }

        if (string.IsNullOrWhiteSpace(input.ProductId))
        {
            throw new DomainValidationException("ProductId は必須です。");
        }

        if (input.Quantity <= 0)
        {
            throw new DomainValidationException("数量は1以上で入力してください。");
        }
    }
}
