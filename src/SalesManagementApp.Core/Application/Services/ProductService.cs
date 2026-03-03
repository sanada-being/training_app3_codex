using System.Collections.Generic;
using System.Linq;
using SalesManagementApp.Core.Application.Exceptions;
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

        if (products.Any(p => p.ProductId == input.ProductId))
        {
            throw new DomainValidationException("同じ商品IDが既に存在します。");
        }

        products.Add(Clone(input));
    }

    public void Update(ICollection<Product> products, Product input)
    {
        Validate(input);

        var target = products.FirstOrDefault(p => p.ProductId == input.ProductId);
        if (target is null)
        {
            throw new DomainValidationException("更新対象の商品が存在しません。");
        }

        target.ProductName = input.ProductName.Trim();
        target.UnitPrice = input.UnitPrice;
        target.Category = input.Category.Trim();
    }

    public void Delete(ICollection<Product> products, string productId)
    {
        if (string.IsNullOrWhiteSpace(productId))
        {
            throw new DomainValidationException("商品IDは必須です。");
        }

        var target = products.FirstOrDefault(p => p.ProductId == productId.Trim());
        if (target is null)
        {
            throw new DomainValidationException("削除対象の商品が存在しません。");
        }

        products.Remove(target);
    }

    public void Validate(Product product)
    {
        if (string.IsNullOrWhiteSpace(product.ProductId))
        {
            throw new DomainValidationException("商品IDは必須です。");
        }

        if (string.IsNullOrWhiteSpace(product.ProductName))
        {
            throw new DomainValidationException("商品名は必須です。");
        }

        if (string.IsNullOrWhiteSpace(product.Category))
        {
            throw new DomainValidationException("区分は必須です。");
        }

        if (product.UnitPrice < 0)
        {
            throw new DomainValidationException("単価は0以上である必要があります。");
        }
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
}
