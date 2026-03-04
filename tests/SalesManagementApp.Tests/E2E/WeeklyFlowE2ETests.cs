using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using SalesManagementApp.Core.Application.Services;
using SalesManagementApp.Core.Domain.Entities;
using SalesManagementApp.Core.Infrastructure.Csv;
using ProductEntity = SalesManagementApp.Core.Domain.Entities.Product;

namespace SalesManagementApp.Tests.E2E;

/// <summary>
/// WeeklyFlowE2E の仕様を検証するNUnitテストクラスです。
/// </summary>
public class WeeklyFlowE2ETests
{
    private string FWorkDir = string.Empty;
    private CsvDataStore FStore = null!;
    private ProductService FProductService = null!;
    private InventoryService FInventoryService = null!;
    private SalesService FSalesService = null!;
    private SalesAggregationService FAggregationService = null!;

    [SetUp]
    /// <summary>
    /// 各テストの実行前にテストデータと依存オブジェクトを初期化します。
    /// </summary>
    public void SetUp()
    {
        FWorkDir = Path.Combine(Path.GetTempPath(), "SalesManagementApp.E2E", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(FWorkDir);
        PrepareTestData();

        FStore = new CsvDataStore();
        FProductService = new ProductService();
        FInventoryService = new InventoryService();
        FSalesService = new SalesService();
        FAggregationService = new SalesAggregationService();
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
    /// テストケース「Weekly Flow Csv Import To Aggregation Completes Successfully」の期待結果を検証します。
    /// </summary>
    public void WeeklyFlow_CsvImportToAggregation_CompletesSuccessfully()
    {
        var wProductsPath = Path.Combine(FWorkDir, "products.csv");
        var wInventoriesPath = Path.Combine(FWorkDir, "inventory.csv");
        var wSalesPath = Path.Combine(FWorkDir, "sales.csv");

        var wProducts = FStore.ReadProducts(wProductsPath).ToList();
        var wInventories = FStore.ReadInventories(wInventoriesPath).ToList();
        var wSales = FStore.ReadSales(wSalesPath).ToList();

        FProductService.Register(wProducts, new ProductEntity
        {
            ProductId = "P003",
            ProductName = "Sandwich",
            UnitPrice = 300,
            Category = "Food"
        });
        FProductService.Update(wProducts, new ProductEntity
        {
            ProductId = "P002",
            ProductName = "Green Tea",
            UnitPrice = 110,
            Category = "Drink"
        });

        FInventoryService.AddStock(wInventories, "S001", "P003", 10);

        FSalesService.RegisterSale(wSales, wProducts, wInventories, new SaleRecord
        {
            SaleDate = new DateTime(2026, 3, 2),
            StoreId = "S001",
            ProductId = "P001",
            Quantity = 3
        });
        FSalesService.RegisterSale(wSales, wProducts, wInventories, new SaleRecord
        {
            SaleDate = new DateTime(2026, 3, 3),
            StoreId = "S001",
            ProductId = "P003",
            Quantity = 2
        });
        FSalesService.RegisterSale(wSales, wProducts, wInventories, new SaleRecord
        {
            SaleDate = new DateTime(2026, 3, 4),
            StoreId = "S001",
            ProductId = "P002",
            Quantity = 5
        });

        FStore.WriteProducts(wProductsPath, wProducts);
        FStore.WriteInventories(wInventoriesPath, wInventories);
        FStore.WriteSales(wSalesPath, wSales);

        var wLoadedSales = FStore.ReadSales(wSalesPath);
        var wTotal = FAggregationService.GetTotalSalesAmount(wLoadedSales, new DateTime(2026, 3, 1), new DateTime(2026, 3, 7));
        var wProductSummaries = FAggregationService.GetProductSummaries(wLoadedSales, new DateTime(2026, 3, 1), new DateTime(2026, 3, 7));
        var wWeeklySummaries = FAggregationService.GetWeeklySummaries(wLoadedSales, new DateTime(2026, 3, 1), new DateTime(2026, 3, 7));
        var wReorderTargets = FInventoryService.GetReorderTargets(wInventories, 8);

        Assert.That(wTotal, Is.EqualTo(1510));
        Assert.That(wProductSummaries.Count, Is.EqualTo(3));
        Assert.That(wProductSummaries.Single(vS => vS.ProductId == "P001").TotalSalesAmount, Is.EqualTo(360));
        Assert.That(wProductSummaries.Single(vS => vS.ProductId == "P002").TotalSalesAmount, Is.EqualTo(550));
        Assert.That(wProductSummaries.Single(vS => vS.ProductId == "P003").TotalSalesAmount, Is.EqualTo(600));
        Assert.That(wWeeklySummaries.Count, Is.EqualTo(1));
        Assert.That(wWeeklySummaries[0].TotalSalesAmount, Is.EqualTo(1510));
        Assert.That(wReorderTargets.Any(vR => vR.ProductId == "P003"), Is.True);
    }

    private void PrepareTestData()
    {
        var wSourceDir = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestData", "E2E");
        File.Copy(Path.Combine(wSourceDir, "products.csv"), Path.Combine(FWorkDir, "products.csv"), overwrite: true);
        File.Copy(Path.Combine(wSourceDir, "inventory.csv"), Path.Combine(FWorkDir, "inventory.csv"), overwrite: true);
        File.Copy(Path.Combine(wSourceDir, "sales.csv"), Path.Combine(FWorkDir, "sales.csv"), overwrite: true);
    }
}
