using System;

namespace SalesManagementApp.Core.Domain.Entities;

public class InventoryHistoryRecord
{
    public DateTime OccurredAt { get; set; }

    public InventoryOperationType OperationType { get; set; }

    public string StoreId { get; set; } = string.Empty;

    public string ProductId { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public int ResultStock { get; set; }

    public string Result { get; set; } = string.Empty;
}
