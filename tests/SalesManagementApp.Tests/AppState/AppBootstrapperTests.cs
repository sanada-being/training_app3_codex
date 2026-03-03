using System;
using System.IO;
using NUnit.Framework;
using SalesManagementApp.Core.Application.State;

namespace SalesManagementApp.Tests.AppState;

public class AppBootstrapperTests
{
    private string _workDir = string.Empty;
    private AppBootstrapper _bootstrapper = null!;

    [SetUp]
    public void SetUp()
    {
        _workDir = Path.Combine(Path.GetTempPath(), "SalesManagementApp.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_workDir);
        _bootstrapper = new AppBootstrapper();
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(_workDir))
        {
            Directory.Delete(_workDir, true);
        }
    }

    [Test]
    public void LoadFromBaseDirectory_WhenRepositoryRootExists_LoadsStateAndResolvesLatestSalesFile()
    {
        var root = Path.Combine(_workDir, "repo");
        Directory.CreateDirectory(root);
        File.WriteAllText(Path.Combine(root, "AGENTS.md"), "marker");
        File.WriteAllLines(Path.Combine(root, "products.csv"), new[]
        {
            "ProductId,ProductName,UnitPrice,Category",
            "P001,Cola,120,Drink"
        });
        File.WriteAllLines(Path.Combine(root, "inventory.csv"), new[]
        {
            "StoreId,ProductId,Stock",
            "S001,P001,10"
        });
        File.WriteAllLines(Path.Combine(root, "sales_20260301.csv"), new[]
        {
            "SaleDate,StoreId,ProductId,Quantity,SalesAmount",
            "2026-03-01,S001,P001,1,120"
        });
        File.WriteAllLines(Path.Combine(root, "sales_20260302.csv"), new[]
        {
            "SaleDate,StoreId,ProductId,Quantity,SalesAmount",
            "2026-03-02,S001,P001,2,240"
        });
        File.WriteAllLines(Path.Combine(root, "inventory_history.csv"), new[]
        {
            "OccurredAt,OperationType,StoreId,ProductId,Quantity,ResultStock,Result",
            "2026-03-02 09:00:00,Sale,S001,P001,2,8,Success"
        });

        var nestedBase = Path.Combine(root, "Sales Management App", "Sales Management App", "bin", "Debug");
        Directory.CreateDirectory(nestedBase);

        var state = _bootstrapper.LoadFromBaseDirectory(nestedBase);

        Assert.That(state.RepositoryRootPath, Is.EqualTo(root));
        Assert.That(state.Products.Count, Is.EqualTo(1));
        Assert.That(state.Inventories.Count, Is.EqualTo(1));
        Assert.That(state.Sales.Count, Is.EqualTo(1));
        Assert.That(state.Sales[0].SaleDate, Is.EqualTo(new DateTime(2026, 3, 2)));
        Assert.That(state.InventoryHistories.Count, Is.EqualTo(1));
        Assert.That(state.SalesPath, Does.EndWith("sales_20260302.csv"));
    }

    [Test]
    public void LoadFromBaseDirectory_WhenRepositoryRootNotFound_ReturnsEmptyState()
    {
        var baseDir = Path.Combine(_workDir, "standalone");
        Directory.CreateDirectory(baseDir);

        var state = _bootstrapper.LoadFromBaseDirectory(baseDir);

        Assert.That(state.RepositoryRootPath, Is.Empty);
        Assert.That(state.Products, Is.Empty);
        Assert.That(state.Inventories, Is.Empty);
        Assert.That(state.Sales, Is.Empty);
        Assert.That(state.InventoryHistories, Is.Empty);
    }
}
