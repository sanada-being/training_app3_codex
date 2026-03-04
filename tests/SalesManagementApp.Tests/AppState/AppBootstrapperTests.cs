using System;
using System.IO;
using NUnit.Framework;
using SalesManagementApp.Core.Application.State;

namespace SalesManagementApp.Tests.AppState;

/// <summary>
/// AppBootstrapper の仕様を検証するNUnitテストクラスです。
/// </summary>
public class AppBootstrapperTests
{
    private string FWorkDir = string.Empty;
    private AppBootstrapper FBootstrapper = null!;

    [SetUp]
    /// <summary>
    /// 各テストの実行前にテストデータと依存オブジェクトを初期化します。
    /// </summary>
    public void SetUp()
    {
        FWorkDir = Path.Combine(Path.GetTempPath(), "SalesManagementApp.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(FWorkDir);
        FBootstrapper = new AppBootstrapper();
    }

    [TearDown]
    /// <summary>
    /// 各テストの実行後に作業ディレクトリやリソースをクリーンアップします。
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
    /// リポジトリルートが存在する場合に各CSVを読み込み、最新の売上CSVパスを解決できることを検証します。
    /// </summary>
    public void LoadFromBaseDirectory_WhenRepositoryRootExists_LoadsStateAndResolvesLatestSalesFile()
    {
        var wRoot = Path.Combine(FWorkDir, "repo");
        Directory.CreateDirectory(wRoot);
        File.WriteAllText(Path.Combine(wRoot, "AGENTS.md"), "marker");
        File.WriteAllLines(Path.Combine(wRoot, "products.csv"), new[]
        {
            "ProductId,ProductName,UnitPrice,Category",
            "P001,Cola,120,Drink"
        });
        File.WriteAllLines(Path.Combine(wRoot, "inventory.csv"), new[]
        {
            "StoreId,ProductId,Stock",
            "S001,P001,10"
        });
        File.WriteAllLines(Path.Combine(wRoot, "sales_20260301.csv"), new[]
        {
            "SaleDate,StoreId,ProductId,Quantity,SalesAmount",
            "2026-03-01,S001,P001,1,120"
        });
        File.WriteAllLines(Path.Combine(wRoot, "sales_20260302.csv"), new[]
        {
            "SaleDate,StoreId,ProductId,Quantity,SalesAmount",
            "2026-03-02,S001,P001,2,240"
        });
        File.WriteAllLines(Path.Combine(wRoot, "inventory_history.csv"), new[]
        {
            "OccurredAt,OperationType,StoreId,ProductId,Quantity,ResultStock,Result",
            "2026-03-02 09:00:00,Sale,S001,P001,2,8,Success"
        });

        var wNestedBase = Path.Combine(wRoot, "Sales Management App", "Sales Management App", "bin", "Debug");
        Directory.CreateDirectory(wNestedBase);

        var wState = FBootstrapper.LoadFromBaseDirectory(wNestedBase);

        Assert.That(wState.RepositoryRootPath, Is.EqualTo(wRoot));
        Assert.That(wState.Products.Count, Is.EqualTo(1));
        Assert.That(wState.Inventories.Count, Is.EqualTo(1));
        Assert.That(wState.Sales.Count, Is.EqualTo(1));
        Assert.That(wState.Sales[0].SaleDate, Is.EqualTo(new DateTime(2026, 3, 2)));
        Assert.That(wState.InventoryHistories.Count, Is.EqualTo(1));
        Assert.That(wState.SalesPath, Does.EndWith("sales_20260302.csv"));
    }

    [Test]
    /// <summary>
    /// リポジトリルートが見つからない場合に空のアプリケーション状態を返すことを検証します。
    /// </summary>
    public void LoadFromBaseDirectory_WhenRepositoryRootNotFound_ReturnsEmptyState()
    {
        var wBaseDir = Path.Combine(FWorkDir, "standalone");
        Directory.CreateDirectory(wBaseDir);

        var wState = FBootstrapper.LoadFromBaseDirectory(wBaseDir);

        Assert.That(wState.RepositoryRootPath, Is.Empty);
        Assert.That(wState.Products, Is.Empty);
        Assert.That(wState.Inventories, Is.Empty);
        Assert.That(wState.Sales, Is.Empty);
        Assert.That(wState.InventoryHistories, Is.Empty);
    }
}
