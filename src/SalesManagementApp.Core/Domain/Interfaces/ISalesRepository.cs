using System.Collections.Generic;
using SalesManagementApp.Core.Domain.Entities;

namespace SalesManagementApp.Core.Domain.Interfaces;

/// <summary>
/// 売上データ永続化の契約を定義します。
/// </summary>
public interface ISalesRepository
{
    IReadOnlyList<SaleRecord> GetAll();
    void SaveAll(IReadOnlyList<SaleRecord> records);
}
