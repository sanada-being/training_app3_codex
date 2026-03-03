using System;
using System.IO;
using NUnit.Framework;
using SalesManagementApp.Core.Application.Exceptions;
using SalesManagementApp.Core.Domain.Entities;
using SalesManagementApp.Core.Infrastructure.Csv;

namespace SalesManagementApp.Tests.Csv;

public class CsvDataStoreTests
{
    private string _workDir = string.Empty;
    private CsvDataStore _store = null!;

    [SetUp]
    public void SetUp()
    {
        _workDir = Path.Combine(Path.GetTempPath(), "SalesManagementApp.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_workDir);
        _store = new CsvDataStore();
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
    public void ReadProducts_WhenCsvIsValid_ReturnsProducts()
    {
        var path = Path.Combine(_workDir, "products.csv");
        File.WriteAllLines(path, new[]
        {
            "ProductId,ProductName,UnitPrice,Category",
            "P001,Cola,120,Drink"
        });

        var products = _store.ReadProducts(path);

        Assert.That(products.Count, Is.EqualTo(1));
        Assert.That(products[0].ProductId, Is.EqualTo("P001"));
        Assert.That(products[0].UnitPrice, Is.EqualTo(120));
    }

    [Test]
    public void ReadProducts_WhenPriceIsInvalid_ThrowsValidationException()
    {
        var path = Path.Combine(_workDir, "products.csv");
        File.WriteAllLines(path, new[]
        {
            "ProductId,ProductName,UnitPrice,Category",
            "P001,Cola,abc,Drink"
        });

        Assert.That(() => _store.ReadProducts(path), Throws.TypeOf<DomainValidationException>());
    }

    [Test]
    public void ReadSales_WhenDateFormatIsInvalid_ThrowsValidationException()
    {
        var path = Path.Combine(_workDir, "sales.csv");
        File.WriteAllLines(path, new[]
        {
            "SaleDate,StoreId,ProductId,Quantity",
            "2026/01/01,S001,P001,1"
        });

        Assert.That(() => _store.ReadSales(path), Throws.TypeOf<DomainValidationException>());
    }

    [Test]
    public void WriteAndReadInventories_WhenDataIsValid_RoundTrips()
    {
        var path = Path.Combine(_workDir, "inventory.csv");
        var input = new[]
        {
            new InventoryRecord { StoreId = "S001", ProductId = "P001", Stock = 3 }
        };

        _store.WriteInventories(path, input);
        var loaded = _store.ReadInventories(path);

        Assert.That(loaded.Count, Is.EqualTo(1));
        Assert.That(loaded[0].StoreId, Is.EqualTo("S001"));
        Assert.That(loaded[0].Stock, Is.EqualTo(3));
    }
}
