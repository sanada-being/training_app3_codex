using System;
using System.IO;
using NUnit.Framework;
using SalesManagementApp.Core.Application.State;
using SalesManagementApp.Core.Domain.Entities;
using SalesManagementApp.Core.Infrastructure.Csv;
using ProductEntity = SalesManagementApp.Core.Domain.Entities.Product;

namespace SalesManagementApp.Tests.AppState;

/// <summary>
/// AppDataRepositoryTests クラスです。
/// </summary>
public class AppDataRepositoryTests
{
    private string FWorkDir = string.Empty;
    private AppDataRepository FRepository = null!;

    [SetUp]
    /// <summary>
    /// 公開メソッドです。
    /// </summary>
    public void SetUp()
    {
        FWorkDir = Path.Combine(Path.GetTempPath(), "SalesManagementApp.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(FWorkDir);
        FRepository = new AppDataRepository();
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
    public void ReadProductsIfExists_WhenFileDoesNotExist_ReturnsEmpty()
    {
        var path = Path.Combine(FWorkDir, "products.csv");

        var result = FRepository.ReadProductsIfExists(path);

        Assert.That(result, Is.Empty);
    }

    [Test]
    /// <summary>
    /// 公開メソッドです。
    /// </summary>
    public void ReadAndNormalizeSalesIfExists_WhenLegacyHeader_ReturnsSalesAndNormalizesCsv()
    {
        var path = Path.Combine(FWorkDir, "sales.csv");
        File.WriteAllLines(path, new[]
        {
            "SaleDate,StoreId,ProductId,Quantity",
            "2026-03-03,S001,P001,2"
        });
        var products = new[]
        {
            new ProductEntity { ProductId = "P001", ProductName = "Cola", UnitPrice = 120, Category = "Drink" }
        };

        var result = FRepository.ReadAndNormalizeSalesIfExists(path, products);

        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result[0].SalesAmount, Is.EqualTo(240));
        var lines = File.ReadAllLines(path);
        Assert.That(lines[0], Is.EqualTo("SaleDate,StoreId,ProductId,Quantity,SalesAmount"));
    }

    [Test]
    /// <summary>
    /// 公開メソッドです。
    /// </summary>
    public void WriteInventoryHistories_WhenPathIsProvided_WritesCsv()
    {
        var path = Path.Combine(FWorkDir, "inventory_history.csv");
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

        FRepository.WriteInventoryHistories(path, records);

        var reloaded = new CsvDataStore().ReadInventoryHistories(path);
        Assert.That(reloaded.Count, Is.EqualTo(1));
        Assert.That(reloaded[0].OperationType, Is.EqualTo(InventoryOperationType.Inbound));
        Assert.That(reloaded[0].ResultStock, Is.EqualTo(13));
    }
}
