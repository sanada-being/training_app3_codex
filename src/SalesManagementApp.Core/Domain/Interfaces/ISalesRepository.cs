using System.Collections.Generic;
using SalesManagementApp.Core.Domain.Entities;

namespace SalesManagementApp.Core.Domain.Interfaces;

/// <summary>
/// インターフェイスです。
/// </summary>
public interface ISalesRepository
{
    IReadOnlyList<SaleRecord> GetAll();
    void SaveAll(IReadOnlyList<SaleRecord> records);
}
