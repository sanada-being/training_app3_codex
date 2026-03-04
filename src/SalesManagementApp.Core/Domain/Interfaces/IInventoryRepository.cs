using System.Collections.Generic;
using SalesManagementApp.Core.Domain.Entities;

namespace SalesManagementApp.Core.Domain.Interfaces;

/// <summary>
/// 在庫データ永続化の契約を定義します。
/// </summary>
public interface IInventoryRepository
{
    IReadOnlyList<InventoryRecord> GetAll();
    void SaveAll(IReadOnlyList<InventoryRecord> records);
}
