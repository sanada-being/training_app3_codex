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

    public IReadOnlyList<Product> GetFiltered(
        IReadOnlyCollection<Product> products,
        string? productIdFilter,
        string? productNameFilter,
        string? categoryFilter)
    {
        var normalizedIdFilter = NormalizeOptionalFilter(productIdFilter);
        var normalizedNameFilter = NormalizeOptionalFilter(productNameFilter);
        var normalizedCategoryFilter = NormalizeOptionalFilter(categoryFilter);

        var query = products.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(normalizedIdFilter))
        {
            query = query.Where(p => HasIgnoreCaseMatch(p.ProductId, normalizedIdFilter));
        }

        if (!string.IsNullOrWhiteSpace(normalizedNameFilter))
        {
            query = query.Where(p => HasIgnoreCaseMatch(p.ProductName, normalizedNameFilter));
        }

        if (!string.IsNullOrWhiteSpace(normalizedCategoryFilter))
        {
            query = query.Where(p => HasIgnoreCaseMatch(p.Category, normalizedCategoryFilter));
        }

        return query.OrderBy(p => p.ProductId).ToList();
    }

    public void Register(ICollection<Product> products, Product input)
    {
        Validate(input);
        var normalizedId = ValidationGuard.RequireNotEmpty(input.ProductId, "ProductId");
        var normalizedName = NormalizeProductName(input.ProductName);

        if (products.Any(p => p.ProductId == normalizedId))
        {
            throw new DomainValidationException("ProductId already exists.");
        }

        if (products.Any(p => NormalizeProductName(p.ProductName) == normalizedName))
        {
            throw new DomainValidationException("ProductName already exists.");
        }

        products.Add(Clone(input));
    }

    public void Update(ICollection<Product> products, Product input)
    {
        Validate(input);
        var normalizedId = ValidationGuard.RequireNotEmpty(input.ProductId, "ProductId");
        var normalizedName = NormalizeProductName(input.ProductName);

        var target = products.FirstOrDefault(p => p.ProductId == normalizedId);
        if (target is null)
        {
            throw new DomainValidationException("Target product is not found.");
        }

        if (products.Any(p => p.ProductId != normalizedId && NormalizeProductName(p.ProductName) == normalizedName))
        {
            throw new DomainValidationException("ProductName already exists.");
        }

        target.ProductId = normalizedId;
        target.ProductName = input.ProductName.Trim();
        target.UnitPrice = input.UnitPrice;
        target.Category = input.Category.Trim();
    }

    public void Delete(ICollection<Product> products, string productId)
    {
        var id = ValidationGuard.RequireNotEmpty(productId, "ProductId");

        var target = products.FirstOrDefault(p => p.ProductId == id);
        if (target is null)
        {
            throw new DomainValidationException("Target product is not found.");
        }

        products.Remove(target);
    }

    public void Validate(Product product)
    {
        ValidationGuard.RequireNotEmpty(product.ProductId, "ProductId");
        ValidationGuard.RequireNotEmpty(product.ProductName, "ProductName");
        ValidationGuard.RequireNotEmpty(product.Category, "Category");
        ValidationGuard.RequireNonNegative(product.UnitPrice, "UnitPrice");
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
        var normalized = ValidationGuard.RequireNotEmpty(productName, "ProductName")
            .Normalize(NormalizationForm.FormKC)
            .ToUpperInvariant();
        return normalized;
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
