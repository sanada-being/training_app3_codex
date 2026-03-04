using System.Collections.Generic;
using SalesManagementApp.Core.Domain.Entities;

namespace SalesManagementApp.Core.Domain.Interfaces;

/// <summary>
/// インターフェイスです。
/// </summary>
public interface IInventoryRepository
{
    IReadOnlyList<InventoryRecord> GetAll();
    void SaveAll(IReadOnlyList<InventoryRecord> records);
}
