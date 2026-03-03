using System.Collections.Generic;
using System.Linq;
using SalesManagementApp.Core.Application.Exceptions;
using SalesManagementApp.Core.Domain.Entities;

namespace SalesManagementApp.Core.Application.Services;

public class InventoryService
{
    private readonly object _syncRoot = new();

    public IReadOnlyList<InventoryRecord> GetAll(IReadOnlyCollection<InventoryRecord> records)
    {
        return records.OrderBy(r => r.StoreId).ThenBy(r => r.ProductId).ToList();
    }

    public IReadOnlyList<InventoryRecord> GetReorderTargets(IReadOnlyCollection<InventoryRecord> records, int threshold = 5)
    {
        return records.Where(r => r.Stock <= threshold).OrderBy(r => r.StoreId).ThenBy(r => r.ProductId).ToList();
    }

    public void AddStock(ICollection<InventoryRecord> records, string storeId, string productId, int quantity)
    {
        ValidateIds(storeId, productId);
        if (quantity <= 0)
        {
            throw new DomainValidationException("入荷数量は1以上である必要があります。");
        }

        lock (_syncRoot)
        {
            var target = FindOrCreate(records, storeId, productId);
            target.Stock += quantity;
        }
    }

    public void RemoveStock(ICollection<InventoryRecord> records, string storeId, string productId, int quantity)
    {
        ValidateIds(storeId, productId);
        if (quantity <= 0)
        {
            throw new DomainValidationException("出庫数量は1以上である必要があります。");
        }

        lock (_syncRoot)
        {
            var target = records.FirstOrDefault(r => r.StoreId == storeId && r.ProductId == productId);
            if (target is null)
            {
                throw new DomainValidationException("対象在庫が存在しません。");
            }

            if (target.Stock < quantity)
            {
                throw new DomainValidationException("在庫が不足しています。");
            }

            target.Stock -= quantity;
        }
    }

    private static void ValidateIds(string storeId, string productId)
    {
        if (string.IsNullOrWhiteSpace(storeId))
        {
            throw new DomainValidationException("StoreId は必須です。");
        }

        if (string.IsNullOrWhiteSpace(productId))
        {
            throw new DomainValidationException("ProductId は必須です。");
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
}
