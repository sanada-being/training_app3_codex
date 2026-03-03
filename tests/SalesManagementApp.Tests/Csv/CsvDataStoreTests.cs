using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using SalesManagementApp.Core.Application.Exceptions;
using SalesManagementApp.Core.Domain.Entities;
using SalesManagementApp.Core.Infrastructure.Csv;
using ProductEntity = SalesManagementApp.Core.Domain.Entities.Product;

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
            "SaleDate,StoreId,ProductId,Quantity,SalesAmount",
            "2026/01/01,S001,P001,1,120"
        });

        Assert.That(() => _store.ReadSales(path), Throws.TypeOf<DomainValidationException>());
    }

    [Test]
    public void ReadSales_WhenHeaderWithoutSalesAmount_LoadsSalesWithZeroAmount()
    {
        var path = Path.Combine(_workDir, "sales.csv");
        File.WriteAllLines(path, new[]
        {
            "SaleDate,StoreId,ProductId,Quantity",
            "2026-03-01,S001,P001,2"
        });

        var sales = _store.ReadSales(path);

        Assert.That(sales.Count, Is.EqualTo(1));
        Assert.That(sales[0].ProductId, Is.EqualTo("P001"));
        Assert.That(sales[0].Quantity, Is.EqualTo(2));
        Assert.That(sales[0].SalesAmount, Is.EqualTo(0));
    }

    [Test]
    public void ReadAndNormalizeSales_WhenHeaderWithoutSalesAmount_ComputesAmountAndRewritesCsv()
    {
        var path = Path.Combine(_workDir, "sales.csv");
        File.WriteAllLines(path, new[]
        {
            "SaleDate,StoreId,ProductId,Quantity",
            "2026-03-01,S001,P001,2"
        });
        var products = new[]
        {
            new ProductEntity { ProductId = "P001", ProductName = "Cola", UnitPrice = 120, Category = "Drink" }
        };

        var sales = _store.ReadAndNormalizeSales(path, products);

        Assert.That(sales.Count, Is.EqualTo(1));
        Assert.That(sales[0].SalesAmount, Is.EqualTo(240));

        var lines = File.ReadAllLines(path);
        Assert.That(lines[0], Is.EqualTo("SaleDate,StoreId,ProductId,Quantity,SalesAmount"));
        Assert.That(lines[1], Is.EqualTo("2026-03-01,S001,P001,2,240"));
    }

    [Test]
    public void ReadAndNormalizeSales_WhenProductMasterIsMissing_ThrowsValidationException()
    {
        var path = Path.Combine(_workDir, "sales.csv");
        File.WriteAllLines(path, new[]
        {
            "SaleDate,StoreId,ProductId,Quantity",
            "2026-03-01,S001,P999,2"
        });
        var products = new[]
        {
            new ProductEntity { ProductId = "P001", ProductName = "Cola", UnitPrice = 120, Category = "Drink" }
        };

        Assert.That(
            () => _store.ReadAndNormalizeSales(path, products),
            Throws.TypeOf<DomainValidationException>());
    }

    [Test]
    public void ReadAndNormalizeSales_WhenSalesAmountDoesNotMatchUnitPrice_ThrowsValidationException()
    {
        var path = Path.Combine(_workDir, "sales.csv");
        File.WriteAllLines(path, new[]
        {
            "SaleDate,StoreId,ProductId,Quantity,SalesAmount",
            "2026-03-01,S001,P001,2,100"
        });
        var products = new[]
        {
            new ProductEntity { ProductId = "P001", ProductName = "Cola", UnitPrice = 120, Category = "Drink" }
        };

        Assert.That(
            () => _store.ReadAndNormalizeSales(path, products),
            Throws.TypeOf<DomainValidationException>());
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

    [Test]
    public void WriteProducts_WhenTargetExists_CreatesBackup()
    {
        var path = Path.Combine(_workDir, "products.csv");
        _store.WriteProducts(path, new[]
        {
            new ProductEntity { ProductId = "P001", ProductName = "Cola", UnitPrice = 120, Category = "Drink" }
        });

        _store.WriteProducts(path, new[]
        {
            new ProductEntity { ProductId = "P002", ProductName = "Tea", UnitPrice = 100, Category = "Drink" }
        });

        var backups = _store.GetBackups(path);

        Assert.That(backups.Count, Is.GreaterThanOrEqualTo(1));
        Assert.That(File.Exists(backups[0]), Is.True);
    }

    [Test]
    public void RestoreLatestBackup_WhenBackupsExist_RestoresPreviousContent()
    {
        var path = Path.Combine(_workDir, "products.csv");
        _store.WriteProducts(path, new[]
        {
            new ProductEntity { ProductId = "P001", ProductName = "Cola", UnitPrice = 120, Category = "Drink" }
        });
        _store.WriteProducts(path, new[]
        {
            new ProductEntity { ProductId = "P002", ProductName = "Tea", UnitPrice = 100, Category = "Drink" }
        });

        _store.RestoreLatestBackup(path);
        var restored = _store.ReadProducts(path);

        Assert.That(restored.Count, Is.EqualTo(1));
        Assert.That(restored[0].ProductId, Is.EqualTo("P001"));
    }

    [Test]
    public void WriteProducts_WritesOperationLog()
    {
        var path = Path.Combine(_workDir, "products.csv");

        _store.WriteProducts(path, new[]
        {
            new ProductEntity { ProductId = "P001", ProductName = "Cola", UnitPrice = 120, Category = "Drink" }
        });

        var logPath = Path.Combine(_workDir, "logs", "operations.log");
        Assert.That(File.Exists(logPath), Is.True);
        var log = File.ReadAllText(logPath);
        Assert.That(log, Does.Contain("Write succeeded"));
    }

    [Test]
    public async Task ReadProductsAsync_WhenCsvIsValid_ReturnsProducts()
    {
        var path = Path.Combine(_workDir, "products.csv");
        File.WriteAllLines(path, new[]
        {
            "ProductId,ProductName,UnitPrice,Category",
            "P001,Cola,120,Drink"
        });

        var result = await _store.ReadProductsAsync(path);

        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result[0].ProductId, Is.EqualTo("P001"));
    }

    [Test]
    public void WriteProductsAsync_WhenCanceled_ThrowsOperationCanceledException()
    {
        var path = Path.Combine(_workDir, "products.csv");
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        Assert.That(
            async () => await _store.WriteProductsAsync(path, new[]
            {
                new ProductEntity { ProductId = "P001", ProductName = "Cola", UnitPrice = 120, Category = "Drink" }
            }, cts.Token),
            Throws.InstanceOf<OperationCanceledException>());
    }
}
