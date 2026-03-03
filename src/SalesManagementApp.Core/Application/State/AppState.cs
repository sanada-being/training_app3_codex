using System.Collections.Generic;
using SalesManagementApp.Core.Domain.Entities;

namespace SalesManagementApp.Core.Application.State;

public sealed class AppState
{
    public List<Product> Products { get; set; } = new();

    public List<InventoryRecord> Inventories { get; set; } = new();

    public List<InventoryHistoryRecord> InventoryHistories { get; set; } = new();

    public List<SaleRecord> Sales { get; set; } = new();

    public string RepositoryRootPath { get; set; } = string.Empty;

    public string ProductsPath { get; set; } = string.Empty;

    public string InventoryPath { get; set; } = string.Empty;

    public string SalesPath { get; set; } = string.Empty;

    public string InventoryHistoryPath { get; set; } = string.Empty;
}
