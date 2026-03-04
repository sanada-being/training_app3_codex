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
    /// 公開メソッドです。
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
    public void WeeklyFlow_CsvImportToAggregation_CompletesSuccessfully()
    {
        var productsPath = Path.Combine(FWorkDir, "products.csv");
        var inventoriesPath = Path.Combine(FWorkDir, "inventory.csv");
        var salesPath = Path.Combine(FWorkDir, "sales.csv");

        var products = FStore.ReadProducts(productsPath).ToList();
        var inventories = FStore.ReadInventories(inventoriesPath).ToList();
        var sales = FStore.ReadSales(salesPath).ToList();

        FProductService.Register(products, new ProductEntity
        {
            ProductId = "P003",
            ProductName = "Sandwich",
            UnitPrice = 300,
            Category = "Food"
        });
        FProductService.Update(products, new ProductEntity
        {
            ProductId = "P002",
            ProductName = "Green Tea",
            UnitPrice = 110,
            Category = "Drink"
        });

        FInventoryService.AddStock(inventories, "S001", "P003", 10);

        FSalesService.RegisterSale(sales, products, inventories, new SaleRecord
        {
            SaleDate = new DateTime(2026, 3, 2),
            StoreId = "S001",
            ProductId = "P001",
            Quantity = 3
        });
        FSalesService.RegisterSale(sales, products, inventories, new SaleRecord
        {
            SaleDate = new DateTime(2026, 3, 3),
            StoreId = "S001",
            ProductId = "P003",
            Quantity = 2
        });
        FSalesService.RegisterSale(sales, products, inventories, new SaleRecord
        {
            SaleDate = new DateTime(2026, 3, 4),
            StoreId = "S001",
            ProductId = "P002",
            Quantity = 5
        });

        FStore.WriteProducts(productsPath, products);
        FStore.WriteInventories(inventoriesPath, inventories);
        FStore.WriteSales(salesPath, sales);

        var loadedSales = FStore.ReadSales(salesPath);
        var total = FAggregationService.GetTotalSalesAmount(loadedSales, new DateTime(2026, 3, 1), new DateTime(2026, 3, 7));
        var productSummaries = FAggregationService.GetProductSummaries(loadedSales, new DateTime(2026, 3, 1), new DateTime(2026, 3, 7));
        var weeklySummaries = FAggregationService.GetWeeklySummaries(loadedSales, new DateTime(2026, 3, 1), new DateTime(2026, 3, 7));
        var reorderTargets = FInventoryService.GetReorderTargets(inventories, 8);

        Assert.That(total, Is.EqualTo(1510));
        Assert.That(productSummaries.Count, Is.EqualTo(3));
        Assert.That(productSummaries.Single(s => s.ProductId == "P001").TotalSalesAmount, Is.EqualTo(360));
        Assert.That(productSummaries.Single(s => s.ProductId == "P002").TotalSalesAmount, Is.EqualTo(550));
        Assert.That(productSummaries.Single(s => s.ProductId == "P003").TotalSalesAmount, Is.EqualTo(600));
        Assert.That(weeklySummaries.Count, Is.EqualTo(1));
        Assert.That(weeklySummaries[0].TotalSalesAmount, Is.EqualTo(1510));
        Assert.That(reorderTargets.Any(r => r.ProductId == "P003"), Is.True);
    }

    private void PrepareTestData()
    {
        var sourceDir = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestData", "E2E");
        File.Copy(Path.Combine(sourceDir, "products.csv"), Path.Combine(FWorkDir, "products.csv"), overwrite: true);
        File.Copy(Path.Combine(sourceDir, "inventory.csv"), Path.Combine(FWorkDir, "inventory.csv"), overwrite: true);
        File.Copy(Path.Combine(sourceDir, "sales.csv"), Path.Combine(FWorkDir, "sales.csv"), overwrite: true);
    }
}
