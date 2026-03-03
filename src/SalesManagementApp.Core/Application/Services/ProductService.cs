using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SalesManagementApp.Core.Application.Exceptions;
using SalesManagementApp.Core.Application.Validation;
using SalesManagementApp.Core.Domain.Entities;

namespace SalesManagementApp.Core.Application.Services;

public class ProductService
{
    public IReadOnlyList<Product> GetAll(IReadOnlyCollection<Product> products)
    {
        return products.OrderBy(p => p.ProductId).ToList();
    }

    public void Register(ICollection<Product> products, Product input)
    {
        Validate(input);
        var normalizedId = ValidationGuard.RequireNotEmpty(input.ProductId, "商品ID");
        var normalizedName = NormalizeProductName(input.ProductName);

        if (products.Any(p => p.ProductId == normalizedId))
        {
            throw new DomainValidationException("同一の商品IDが既に登録されています。");
        }

        if (products.Any(p => NormalizeProductName(p.ProductName) == normalizedName))
        {
            throw new DomainValidationException("同一の商品名が既に登録されています。");
        }

        products.Add(Clone(input));
    }

    public void Update(ICollection<Product> products, Product input)
    {
        Validate(input);
        var normalizedId = ValidationGuard.RequireNotEmpty(input.ProductId, "商品ID");
        var normalizedName = NormalizeProductName(input.ProductName);

        var target = products.FirstOrDefault(p => p.ProductId == normalizedId);
        if (target is null)
        {
            throw new DomainValidationException("更新対象の商品が存在しません。");
        }

        if (products.Any(p => p.ProductId != normalizedId && NormalizeProductName(p.ProductName) == normalizedName))
        {
            throw new DomainValidationException("同一の商品名が既に登録されています。");
        }

        target.ProductId = normalizedId;
        target.ProductName = input.ProductName.Trim();
        target.UnitPrice = input.UnitPrice;
        target.Category = input.Category.Trim();
    }

    public void Delete(ICollection<Product> products, string productId)
    {
        var id = ValidationGuard.RequireNotEmpty(productId, "商品ID");

        var target = products.FirstOrDefault(p => p.ProductId == id);
        if (target is null)
        {
            throw new DomainValidationException("削除対象の商品が存在しません。");
        }

        products.Remove(target);
    }

    public void Validate(Product product)
    {
        ValidationGuard.RequireNotEmpty(product.ProductId, "商品ID");
        ValidationGuard.RequireNotEmpty(product.ProductName, "商品名");
        ValidationGuard.RequireNotEmpty(product.Category, "区分");
        ValidationGuard.RequireNonNegative(product.UnitPrice, "単価");
    }

    private static Product Clone(Product input)
    {
        return new Product
        {
            ProductId = input.ProductId.Trim(),
            ProductName = input.ProductName.Trim(),
            UnitPrice = input.UnitPrice,
            Category = input.Category.Trim()
        };
    }

    private static string NormalizeProductName(string productName)
    {
        var normalized = ValidationGuard.RequireNotEmpty(productName, "商品名")
            .Normalize(NormalizationForm.FormKC)
            .ToUpperInvariant();
        return normalized;
    }
}
