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

public class WeeklyFlowE2ETests
{
    private string _workDir = string.Empty;
    private CsvDataStore _store = null!;
    private ProductService _productService = null!;
    private InventoryService _inventoryService = null!;
    private SalesService _salesService = null!;
    private SalesAggregationService _aggregationService = null!;

    [SetUp]
    public void SetUp()
    {
        _workDir = Path.Combine(Path.GetTempPath(), "SalesManagementApp.E2E", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_workDir);
        PrepareTestData();

        _store = new CsvDataStore();
        _productService = new ProductService();
        _inventoryService = new InventoryService();
        _salesService = new SalesService();
        _aggregationService = new SalesAggregationService();
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
    public void WeeklyFlow_CsvImportToAggregation_CompletesSuccessfully()
    {
        var productsPath = Path.Combine(_workDir, "products.csv");
        var inventoriesPath = Path.Combine(_workDir, "inventory.csv");
        var salesPath = Path.Combine(_workDir, "sales.csv");

        var products = _store.ReadProducts(productsPath).ToList();
        var inventories = _store.ReadInventories(inventoriesPath).ToList();
        var sales = _store.ReadSales(salesPath).ToList();

        _productService.Register(products, new ProductEntity
        {
            ProductId = "P003",
            ProductName = "Sandwich",
            UnitPrice = 300,
            Category = "Food"
        });
        _productService.Update(products, new ProductEntity
        {
            ProductId = "P002",
            ProductName = "Green Tea",
            UnitPrice = 110,
            Category = "Drink"
        });

        _inventoryService.AddStock(inventories, "S001", "P003", 10);

        _salesService.RegisterSale(sales, products, inventories, new SaleRecord
        {
            SaleDate = new DateTime(2026, 3, 2),
            StoreId = "S001",
            ProductId = "P001",
            Quantity = 3
        });
        _salesService.RegisterSale(sales, products, inventories, new SaleRecord
        {
            SaleDate = new DateTime(2026, 3, 3),
            StoreId = "S001",
            ProductId = "P003",
            Quantity = 2
        });
        _salesService.RegisterSale(sales, products, inventories, new SaleRecord
        {
            SaleDate = new DateTime(2026, 3, 4),
            StoreId = "S001",
            ProductId = "P002",
            Quantity = 5
        });

        _store.WriteProducts(productsPath, products);
        _store.WriteInventories(inventoriesPath, inventories);
        _store.WriteSales(salesPath, sales);

        var loadedSales = _store.ReadSales(salesPath);
        var total = _aggregationService.GetTotalSalesAmount(loadedSales, new DateTime(2026, 3, 1), new DateTime(2026, 3, 7));
        var productSummaries = _aggregationService.GetProductSummaries(loadedSales, new DateTime(2026, 3, 1), new DateTime(2026, 3, 7));
        var weeklySummaries = _aggregationService.GetWeeklySummaries(loadedSales, new DateTime(2026, 3, 1), new DateTime(2026, 3, 7));
        var reorderTargets = _inventoryService.GetReorderTargets(inventories, 8);

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
        File.Copy(Path.Combine(sourceDir, "products.csv"), Path.Combine(_workDir, "products.csv"), overwrite: true);
        File.Copy(Path.Combine(sourceDir, "inventory.csv"), Path.Combine(_workDir, "inventory.csv"), overwrite: true);
        File.Copy(Path.Combine(sourceDir, "sales.csv"), Path.Combine(_workDir, "sales.csv"), overwrite: true);
    }
}
