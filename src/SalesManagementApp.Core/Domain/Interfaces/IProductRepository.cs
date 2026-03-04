using System.Collections.Generic;
using SalesManagementApp.Core.Domain.Entities;

namespace SalesManagementApp.Core.Domain.Interfaces;

/// <summary>
/// インターフェイスです。
/// </summary>
public interface IProductRepository
{
    IReadOnlyList<Product> GetAll();
    Product? FindById(string productId);
    void SaveAll(IReadOnlyList<Product> products);
}
