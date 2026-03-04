using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SalesManagementApp.Core.Application.Exceptions;
using SalesManagementApp.Core.Application.Validation;
using SalesManagementApp.Core.Domain.Entities;

namespace SalesManagementApp.Core.Application.Services;

/// <summary>
/// 商品マスタの検索・登録・更新・削除を扱うアプリケーションサービスです。
/// </summary>
public class ProductService
{
    /// <summary>
    /// 商品一覧を商品ID順に並べて返します。
    /// </summary>
    public IReadOnlyList<Product> GetAll(IReadOnlyCollection<Product> vProducts)
    {
        return vProducts.OrderBy(vP => vP.ProductId).ToList();
    }

    /// <summary>
    /// 商品ID・商品名・カテゴリ条件で商品一覧を絞り込みます。
    /// </summary>
    public IReadOnlyList<Product> GetFiltered(
        IReadOnlyCollection<Product> vProducts,
        string? vProductIdFilter,
        string? vProductNameFilter,
        string? vCategoryFilter)
    {
        var wNormalizedIdFilter = NormalizeOptionalFilter(vProductIdFilter);
        var wNormalizedNameFilter = NormalizeOptionalFilter(vProductNameFilter);
        var wNormalizedCategoryFilter = NormalizeOptionalFilter(vCategoryFilter);

        var wQuery = vProducts.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(wNormalizedIdFilter))
        {
            wQuery = wQuery.Where(vP => HasIgnoreCaseMatch(vP.ProductId, wNormalizedIdFilter));
        }

        if (!string.IsNullOrWhiteSpace(wNormalizedNameFilter))
        {
            wQuery = wQuery.Where(vP => HasIgnoreCaseMatch(vP.ProductName, wNormalizedNameFilter));
        }

        if (!string.IsNullOrWhiteSpace(wNormalizedCategoryFilter))
        {
            wQuery = wQuery.Where(vP => HasIgnoreCaseMatch(vP.Category, wNormalizedCategoryFilter));
        }

        return wQuery.OrderBy(vP => vP.ProductId).ToList();
    }

    /// <summary>
    /// 入力内容を検証し、商品を登録します。
    /// </summary>
    public void Register(ICollection<Product> vProducts, Product vInput)
    {
        Validate(vInput);
        var wNormalizedId = ValidationGuard.RequireNotEmpty(vInput.ProductId, "ProductId");
        var wNormalizedName = NormalizeProductName(vInput.ProductName);

        if (vProducts.Any(vP => vP.ProductId == wNormalizedId))
        {
            throw new DomainValidationException("ProductId already exists.");
        }

        if (vProducts.Any(vP => NormalizeProductName(vP.ProductName) == wNormalizedName))
        {
            throw new DomainValidationException("ProductName already exists.");
        }

        vProducts.Add(Clone(vInput));
    }

    /// <summary>
    /// 指定した商品IDの商品情報を更新します。
    /// </summary>
    public void Update(ICollection<Product> vProducts, Product vInput)
    {
        Validate(vInput);
        var wNormalizedId = ValidationGuard.RequireNotEmpty(vInput.ProductId, "ProductId");
        var wNormalizedName = NormalizeProductName(vInput.ProductName);

        var wTarget = vProducts.FirstOrDefault(vP => vP.ProductId == wNormalizedId);
        if (wTarget is null)
        {
            throw new DomainValidationException("Target product is not found.");
        }

        if (vProducts.Any(vP => vP.ProductId != wNormalizedId && NormalizeProductName(vP.ProductName) == wNormalizedName))
        {
            throw new DomainValidationException("ProductName already exists.");
        }

        wTarget.ProductId = wNormalizedId;
        wTarget.ProductName = vInput.ProductName.Trim();
        wTarget.UnitPrice = vInput.UnitPrice;
        wTarget.Category = vInput.Category.Trim();
    }

    /// <summary>
    /// 指定した商品IDの商品を削除します。
    /// </summary>
    public void Delete(ICollection<Product> vProducts, string vProductId)
    {
        var wId = ValidationGuard.RequireNotEmpty(vProductId, "ProductId");

        var wTarget = vProducts.FirstOrDefault(vP => vP.ProductId == wId);
        if (wTarget is null)
        {
            throw new DomainValidationException("Target product is not found.");
        }

        vProducts.Remove(wTarget);
    }

    /// <summary>
    /// 商品情報の必須項目と値域を検証します。
    /// </summary>
    public void Validate(Product vProduct)
    {
        ValidationGuard.RequireNotEmpty(vProduct.ProductId, "ProductId");
        ValidationGuard.RequireNotEmpty(vProduct.ProductName, "ProductName");
        ValidationGuard.RequireNotEmpty(vProduct.Category, "Category");
        ValidationGuard.RequireNonNegative(vProduct.UnitPrice, "UnitPrice");
    }

    private static Product Clone(Product vInput)
    {
        return new Product
        {
            ProductId = vInput.ProductId.Trim(),
            ProductName = vInput.ProductName.Trim(),
            UnitPrice = vInput.UnitPrice,
            Category = vInput.Category.Trim()
        };
    }

    private static string NormalizeProductName(string vProductName)
    {
        var wNormalized = ValidationGuard.RequireNotEmpty(vProductName, "ProductName")
            .Normalize(NormalizationForm.FormKC)
            .ToUpperInvariant();
        return wNormalized;
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
