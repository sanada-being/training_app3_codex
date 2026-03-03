using System.Collections.Generic;
using SalesManagementApp.Core.Domain.Entities;

namespace SalesManagementApp.Core.Domain.Interfaces;

public interface IInventoryRepository
{
    IReadOnlyList<InventoryRecord> GetAll();
    void SaveAll(IReadOnlyList<InventoryRecord> records);
}
