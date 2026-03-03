using System;
using System.IO;
using NUnit.Framework;
using SalesManagementApp.Core.Application.State;
using SalesManagementApp.Core.Domain.Entities;
using SalesManagementApp.Core.Infrastructure.Csv;
using ProductEntity = SalesManagementApp.Core.Domain.Entities.Product;

namespace SalesManagementApp.Tests.AppState;

public class AppDataRepositoryTests
{
    private string _workDir = string.Empty;
    private AppDataRepository _repository = null!;

    [SetUp]
    public void SetUp()
    {
        _workDir = Path.Combine(Path.GetTempPath(), "SalesManagementApp.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_workDir);
        _repository = new AppDataRepository();
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
    public void ReadProductsIfExists_WhenFileDoesNotExist_ReturnsEmpty()
    {
        var path = Path.Combine(_workDir, "products.csv");

        var result = _repository.ReadProductsIfExists(path);

        Assert.That(result, Is.Empty);
    }

    [Test]
    public void ReadAndNormalizeSalesIfExists_WhenLegacyHeader_ReturnsSalesAndNormalizesCsv()
    {
        var path = Path.Combine(_workDir, "sales.csv");
        File.WriteAllLines(path, new[]
        {
            "SaleDate,StoreId,ProductId,Quantity",
            "2026-03-03,S001,P001,2"
        });
        var products = new[]
        {
            new ProductEntity { ProductId = "P001", ProductName = "Cola", UnitPrice = 120, Category = "Drink" }
        };

        var result = _repository.ReadAndNormalizeSalesIfExists(path, products);

        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result[0].SalesAmount, Is.EqualTo(240));
        var lines = File.ReadAllLines(path);
        Assert.That(lines[0], Is.EqualTo("SaleDate,StoreId,ProductId,Quantity,SalesAmount"));
    }

    [Test]
    public void WriteInventoryHistories_WhenPathIsProvided_WritesCsv()
    {
        var path = Path.Combine(_workDir, "inventory_history.csv");
        var records = new[]
        {
            new InventoryHistoryRecord
            {
                OccurredAt = new DateTime(2026, 3, 3, 8, 0, 0),
                OperationType = InventoryOperationType.Inbound,
                StoreId = "S001",
                ProductId = "P001",
                Quantity = 3,
                ResultStock = 13,
                Result = "Success"
            }
        };

        _repository.WriteInventoryHistories(path, records);

        var reloaded = new CsvDataStore().ReadInventoryHistories(path);
        Assert.That(reloaded.Count, Is.EqualTo(1));
        Assert.That(reloaded[0].OperationType, Is.EqualTo(InventoryOperationType.Inbound));
        Assert.That(reloaded[0].ResultStock, Is.EqualTo(13));
    }
}
