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

/// <summary>
/// CsvDataStoreTests クラスです。
/// </summary>
public class CsvDataStoreTests
{
    private string FWorkDir = string.Empty;
    private CsvDataStore FStore = null!;

    [SetUp]
    /// <summary>
    /// 公開メソッドです。
    /// </summary>
    public void SetUp()
    {
        FWorkDir = Path.Combine(Path.GetTempPath(), "SalesManagementApp.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(FWorkDir);
        FStore = new CsvDataStore();
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
    public void ReadProducts_WhenCsvIsValid_ReturnsProducts()
    {
        var path = Path.Combine(FWorkDir, "products.csv");
        File.WriteAllLines(path, new[]
        {
            "ProductId,ProductName,UnitPrice,Category",
            "P001,Cola,120,Drink"
        });

        var products = FStore.ReadProducts(path);

        Assert.That(products.Count, Is.EqualTo(1));
        Assert.That(products[0].ProductId, Is.EqualTo("P001"));
        Assert.That(products[0].UnitPrice, Is.EqualTo(120));
    }

    [Test]
    /// <summary>
    /// 公開メソッドです。
    /// </summary>
    public void ReadProducts_WhenPriceIsInvalid_ThrowsValidationException()
    {
        var path = Path.Combine(FWorkDir, "products.csv");
        File.WriteAllLines(path, new[]
        {
            "ProductId,ProductName,UnitPrice,Category",
            "P001,Cola,abc,Drink"
        });

        Assert.That(() => FStore.ReadProducts(path), Throws.TypeOf<DomainValidationException>());
    }

    [Test]
    /// <summary>
    /// 公開メソッドです。
    /// </summary>
    public void ReadSales_WhenDateFormatIsInvalid_ThrowsValidationException()
    {
        var path = Path.Combine(FWorkDir, "sales.csv");
        File.WriteAllLines(path, new[]
        {
            "SaleDate,StoreId,ProductId,Quantity,SalesAmount",
            "2026/01/01,S001,P001,1,120"
        });

        Assert.That(() => FStore.ReadSales(path), Throws.TypeOf<DomainValidationException>());
    }

    [Test]
    /// <summary>
    /// 公開メソッドです。
    /// </summary>
    public void ReadSales_WhenHeaderWithoutSalesAmount_LoadsSalesWithZeroAmount()
    {
        var path = Path.Combine(FWorkDir, "sales.csv");
        File.WriteAllLines(path, new[]
        {
            "SaleDate,StoreId,ProductId,Quantity",
            "2026-03-01,S001,P001,2"
        });

        var sales = FStore.ReadSales(path);

        Assert.That(sales.Count, Is.EqualTo(1));
        Assert.That(sales[0].ProductId, Is.EqualTo("P001"));
        Assert.That(sales[0].Quantity, Is.EqualTo(2));
        Assert.That(sales[0].SalesAmount, Is.EqualTo(0));
    }

    [Test]
    /// <summary>
    /// 公開メソッドです。
    /// </summary>
    public void ReadAndNormalizeSales_WhenHeaderWithoutSalesAmount_ComputesAmountAndRewritesCsv()
    {
        var path = Path.Combine(FWorkDir, "sales.csv");
        File.WriteAllLines(path, new[]
        {
            "SaleDate,StoreId,ProductId,Quantity",
            "2026-03-01,S001,P001,2"
        });
        var products = new[]
        {
            new ProductEntity { ProductId = "P001", ProductName = "Cola", UnitPrice = 120, Category = "Drink" }
        };

        var sales = FStore.ReadAndNormalizeSales(path, products);

        Assert.That(sales.Count, Is.EqualTo(1));
        Assert.That(sales[0].SalesAmount, Is.EqualTo(240));

        var lines = File.ReadAllLines(path);
        Assert.That(lines[0], Is.EqualTo("SaleDate,StoreId,ProductId,Quantity,SalesAmount"));
        Assert.That(lines[1], Is.EqualTo("2026-03-01,S001,P001,2,240"));
    }

    [Test]
    /// <summary>
    /// 公開メソッドです。
    /// </summary>
    public void ReadAndNormalizeSales_WhenProductMasterIsMissing_ThrowsValidationException()
    {
        var path = Path.Combine(FWorkDir, "sales.csv");
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
            () => FStore.ReadAndNormalizeSales(path, products),
            Throws.TypeOf<DomainValidationException>());
    }

    [Test]
    /// <summary>
    /// 公開メソッドです。
    /// </summary>
    public void ReadAndNormalizeSales_WhenSalesAmountDoesNotMatchUnitPrice_ThrowsValidationException()
    {
        var path = Path.Combine(FWorkDir, "sales.csv");
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
            () => FStore.ReadAndNormalizeSales(path, products),
            Throws.TypeOf<DomainValidationException>());
    }

    [Test]
    /// <summary>
    /// 公開メソッドです。
    /// </summary>
    public void WriteAndReadInventories_WhenDataIsValid_RoundTrips()
    {
        var path = Path.Combine(FWorkDir, "inventory.csv");
        var input = new[]
        {
            new InventoryRecord { StoreId = "S001", ProductId = "P001", Stock = 3 }
        };

        FStore.WriteInventories(path, input);
        var loaded = FStore.ReadInventories(path);

        Assert.That(loaded.Count, Is.EqualTo(1));
        Assert.That(loaded[0].StoreId, Is.EqualTo("S001"));
        Assert.That(loaded[0].Stock, Is.EqualTo(3));
    }

    [Test]
    /// <summary>
    /// 公開メソッドです。
    /// </summary>
    public void WriteAndReadInventoryHistories_WhenDataIsValid_RoundTrips()
    {
        var path = Path.Combine(FWorkDir, "inventory_history.csv");
        var input = new[]
        {
            new InventoryHistoryRecord
            {
                OccurredAt = new DateTime(2026, 3, 3, 10, 15, 0),
                OperationType = InventoryOperationType.Inbound,
                StoreId = "S001",
                ProductId = "P001",
                Quantity = 5,
                ResultStock = 15,
                Result = "Success"
            }
        };

        FStore.WriteInventoryHistories(path, input);
        var loaded = FStore.ReadInventoryHistories(path);

        Assert.That(loaded.Count, Is.EqualTo(1));
        Assert.That(loaded[0].OccurredAt, Is.EqualTo(new DateTime(2026, 3, 3, 10, 15, 0)));
        Assert.That(loaded[0].OperationType, Is.EqualTo(InventoryOperationType.Inbound));
        Assert.That(loaded[0].StoreId, Is.EqualTo("S001"));
        Assert.That(loaded[0].ProductId, Is.EqualTo("P001"));
        Assert.That(loaded[0].Quantity, Is.EqualTo(5));
        Assert.That(loaded[0].ResultStock, Is.EqualTo(15));
        Assert.That(loaded[0].Result, Is.EqualTo("Success"));
    }

    [Test]
    /// <summary>
    /// 公開メソッドです。
    /// </summary>
    public void WriteProducts_WhenTargetExists_CreatesBackup()
    {
        var path = Path.Combine(FWorkDir, "products.csv");
        FStore.WriteProducts(path, new[]
        {
            new ProductEntity { ProductId = "P001", ProductName = "Cola", UnitPrice = 120, Category = "Drink" }
        });

        FStore.WriteProducts(path, new[]
        {
            new ProductEntity { ProductId = "P002", ProductName = "Tea", UnitPrice = 100, Category = "Drink" }
        });

        var backups = FStore.GetBackups(path);

        Assert.That(backups.Count, Is.GreaterThanOrEqualTo(1));
        Assert.That(File.Exists(backups[0]), Is.True);
    }

    [Test]
    /// <summary>
    /// 公開メソッドです。
    /// </summary>
    public void RestoreLatestBackup_WhenBackupsExist_RestoresPreviousContent()
    {
        var path = Path.Combine(FWorkDir, "products.csv");
        FStore.WriteProducts(path, new[]
        {
            new ProductEntity { ProductId = "P001", ProductName = "Cola", UnitPrice = 120, Category = "Drink" }
        });
        FStore.WriteProducts(path, new[]
        {
            new ProductEntity { ProductId = "P002", ProductName = "Tea", UnitPrice = 100, Category = "Drink" }
        });

        FStore.RestoreLatestBackup(path);
        var restored = FStore.ReadProducts(path);

        Assert.That(restored.Count, Is.EqualTo(1));
        Assert.That(restored[0].ProductId, Is.EqualTo("P001"));
    }

    [Test]
    /// <summary>
    /// 公開メソッドです。
    /// </summary>
    public void WriteProducts_WritesOperationLog()
    {
        var path = Path.Combine(FWorkDir, "products.csv");

        FStore.WriteProducts(path, new[]
        {
            new ProductEntity { ProductId = "P001", ProductName = "Cola", UnitPrice = 120, Category = "Drink" }
        });

        var logPath = Path.Combine(FWorkDir, "logs", "operations.log");
        Assert.That(File.Exists(logPath), Is.True);
        var log = File.ReadAllText(logPath);
        Assert.That(log, Does.Contain("Write succeeded"));
    }

    [Test]
    /// <summary>
    /// 公開メソッドです。
    /// </summary>
    public async Task ReadProductsAsync_WhenCsvIsValid_ReturnsProducts()
    {
        var path = Path.Combine(FWorkDir, "products.csv");
        File.WriteAllLines(path, new[]
        {
            "ProductId,ProductName,UnitPrice,Category",
            "P001,Cola,120,Drink"
        });

        var result = await FStore.ReadProductsAsync(path);

        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result[0].ProductId, Is.EqualTo("P001"));
    }

    [Test]
    /// <summary>
    /// 公開メソッドです。
    /// </summary>
    public void WriteProductsAsync_WhenCanceled_ThrowsOperationCanceledException()
    {
        var path = Path.Combine(FWorkDir, "products.csv");
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        Assert.That(
            async () => await FStore.WriteProductsAsync(path, new[]
            {
                new ProductEntity { ProductId = "P001", ProductName = "Cola", UnitPrice = 120, Category = "Drink" }
            }, cts.Token),
            Throws.InstanceOf<OperationCanceledException>());
    }
}
