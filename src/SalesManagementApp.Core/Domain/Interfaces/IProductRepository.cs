using System.Collections.Generic;
using SalesManagementApp.Core.Domain.Entities;

namespace SalesManagementApp.Core.Domain.Interfaces;

/// <summary>
/// 商品データ永続化の契約を定義します。
/// </summary>
public interface IProductRepository
{
    IReadOnlyList<Product> GetAll();
    Product? FindById(string vProductId);
    void SaveAll(IReadOnlyList<Product> vProducts);
}
