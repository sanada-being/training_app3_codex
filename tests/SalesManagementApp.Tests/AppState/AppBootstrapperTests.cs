using System;
using System.IO;
using NUnit.Framework;
using SalesManagementApp.Core.Application.State;

namespace SalesManagementApp.Tests.AppState;

/// <summary>
/// AppBootstrapperTests クラスです。
/// </summary>
public class AppBootstrapperTests
{
    private string FWorkDir = string.Empty;
    private AppBootstrapper FBootstrapper = null!;

    [SetUp]
    /// <summary>
    /// 公開メソッドです。
    /// </summary>
    public void SetUp()
    {
        FWorkDir = Path.Combine(Path.GetTempPath(), "SalesManagementApp.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(FWorkDir);
        FBootstrapper = new AppBootstrapper();
    }

    [TearDown]
    /// <summary>
    /// 公開メソッドです。
    /// </summary>
    public void TearDown()
    {
        if (Directory.Exists(FWorkDir))
        {
            Directory.Delete(FWorkDir, true);
        }
    }

    [Test]
    /// <summary>
    /// 公開メソッドです。
    /// </summary>
    public void LoadFromBaseDirectory_WhenRepositoryRootExists_LoadsStateAndResolvesLatestSalesFile()
    {
        var root = Path.Combine(FWorkDir, "repo");
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

        var state = FBootstrapper.LoadFromBaseDirectory(nestedBase);

        Assert.That(state.RepositoryRootPath, Is.EqualTo(root));
        Assert.That(state.Products.Count, Is.EqualTo(1));
        Assert.That(state.Inventories.Count, Is.EqualTo(1));
        Assert.That(state.Sales.Count, Is.EqualTo(1));
        Assert.That(state.Sales[0].SaleDate, Is.EqualTo(new DateTime(2026, 3, 2)));
        Assert.That(state.InventoryHistories.Count, Is.EqualTo(1));
        Assert.That(state.SalesPath, Does.EndWith("sales_20260302.csv"));
    }

    [Test]
    /// <summary>
    /// 公開メソッドです。
    /// </summary>
    public void LoadFromBaseDirectory_WhenRepositoryRootNotFound_ReturnsEmptyState()
    {
        var baseDir = Path.Combine(FWorkDir, "standalone");
        Directory.CreateDirectory(baseDir);

        var state = FBootstrapper.LoadFromBaseDirectory(baseDir);

        Assert.That(state.RepositoryRootPath, Is.Empty);
        Assert.That(state.Products, Is.Empty);
        Assert.That(state.Inventories, Is.Empty);
        Assert.That(state.Sales, Is.Empty);
        Assert.That(state.InventoryHistories, Is.Empty);
    }
}
