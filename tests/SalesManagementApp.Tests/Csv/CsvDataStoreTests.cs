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
/// CsvDataStore の仕様を検証するNUnitテストクラスです。
/// </summary>
public class CsvDataStoreTests
{
    private string FWorkDir = string.Empty;
    private CsvDataStore FStore = null!;

    [SetUp]
    /// <summary>
    /// 各テストの実行前にテストデータと依存オブジェクトを初期化します。
    /// </summary>
    public void SetUp()
    {
        FWorkDir = Path.Combine(Path.GetTempPath(), "SalesManagementApp.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(FWorkDir);
        FStore = new CsvDataStore();
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
    /// 正常な商品CSVを読み込んだ場合に商品一覧を取得できることを検証します。
    /// </summary>
    public void ReadProducts_WhenCsvIsValid_ReturnsProducts()
    {
        var wPath = Path.Combine(FWorkDir, "products.csv");
        File.WriteAllLines(wPath, new[]
        {
            "ProductId,ProductName,UnitPrice,Category",
            "P001,Cola,120,Drink"
        });

        var wProducts = FStore.ReadProducts(wPath);

        Assert.That(wProducts.Count, Is.EqualTo(1));
        Assert.That(wProducts[0].ProductId, Is.EqualTo("P001"));
        Assert.That(wProducts[0].UnitPrice, Is.EqualTo(120));
    }

    [Test]
    /// <summary>
    /// 商品CSVの単価が不正な場合に検証例外が発生することを確認します。
    /// </summary>
    public void ReadProducts_WhenPriceIsInvalid_ThrowsValidationException()
    {
        var wPath = Path.Combine(FWorkDir, "products.csv");
        File.WriteAllLines(wPath, new[]
        {
            "ProductId,ProductName,UnitPrice,Category",
            "P001,Cola,abc,Drink"
        });

        Assert.That(() => FStore.ReadProducts(wPath), Throws.TypeOf<DomainValidationException>());
    }

    [Test]
    /// <summary>
    /// 売上CSVの日付形式が不正な場合に検証例外が発生することを確認します。
    /// </summary>
    public void ReadSales_WhenDateFormatIsInvalid_ThrowsValidationException()
    {
        var wPath = Path.Combine(FWorkDir, "sales.csv");
        File.WriteAllLines(wPath, new[]
        {
            "SaleDate,StoreId,ProductId,Quantity,SalesAmount",
            "2026/01/01,S001,P001,1,120"
        });

        Assert.That(() => FStore.ReadSales(wPath), Throws.TypeOf<DomainValidationException>());
    }

    [Test]
    /// <summary>
    /// 売上金額列がない売上CSVを読み込んだ場合に売上金額が0で取り込まれることを検証します。
    /// </summary>
    public void ReadSales_WhenHeaderWithoutSalesAmount_LoadsSalesWithZeroAmount()
    {
        var wPath = Path.Combine(FWorkDir, "sales.csv");
        File.WriteAllLines(wPath, new[]
        {
            "SaleDate,StoreId,ProductId,Quantity",
            "2026-03-01,S001,P001,2"
        });

        var wSales = FStore.ReadSales(wPath);

        Assert.That(wSales.Count, Is.EqualTo(1));
        Assert.That(wSales[0].ProductId, Is.EqualTo("P001"));
        Assert.That(wSales[0].Quantity, Is.EqualTo(2));
        Assert.That(wSales[0].SalesAmount, Is.EqualTo(0));
    }

    [Test]
    /// <summary>
    /// 売上金額列がない売上CSVを正規化し、計算した金額でCSVを書き換えることを検証します。
    /// </summary>
    public void ReadAndNormalizeSales_WhenHeaderWithoutSalesAmount_ComputesAmountAndRewritesCsv()
    {
        var wPath = Path.Combine(FWorkDir, "sales.csv");
        File.WriteAllLines(wPath, new[]
        {
            "SaleDate,StoreId,ProductId,Quantity",
            "2026-03-01,S001,P001,2"
        });
        var wProducts = new[]
        {
            new ProductEntity { ProductId = "P001", ProductName = "Cola", UnitPrice = 120, Category = "Drink" }
        };

        var wSales = FStore.ReadAndNormalizeSales(wPath, wProducts);

        Assert.That(wSales.Count, Is.EqualTo(1));
        Assert.That(wSales[0].SalesAmount, Is.EqualTo(240));

        var wLines = File.ReadAllLines(wPath);
        Assert.That(wLines[0], Is.EqualTo("SaleDate,StoreId,ProductId,Quantity,SalesAmount"));
        Assert.That(wLines[1], Is.EqualTo("2026-03-01,S001,P001,2,240"));
    }

    [Test]
    /// <summary>
    /// 売上CSVに商品マスタ未登録の商品IDが含まれる場合に検証例外が発生することを確認します。
    /// </summary>
    public void ReadAndNormalizeSales_WhenProductMasterIsMissing_ThrowsValidationException()
    {
        var wPath = Path.Combine(FWorkDir, "sales.csv");
        File.WriteAllLines(wPath, new[]
        {
            "SaleDate,StoreId,ProductId,Quantity",
            "2026-03-01,S001,P999,2"
        });
        var wProducts = new[]
        {
            new ProductEntity { ProductId = "P001", ProductName = "Cola", UnitPrice = 120, Category = "Drink" }
        };

        Assert.That(
            () => FStore.ReadAndNormalizeSales(wPath, wProducts),
            Throws.TypeOf<DomainValidationException>());
    }

    [Test]
    /// <summary>
    /// 売上金額が単価×数量と一致しない場合に検証例外が発生することを確認します。
    /// </summary>
    public void ReadAndNormalizeSales_WhenSalesAmountDoesNotMatchUnitPrice_ThrowsValidationException()
    {
        var wPath = Path.Combine(FWorkDir, "sales.csv");
        File.WriteAllLines(wPath, new[]
        {
            "SaleDate,StoreId,ProductId,Quantity,SalesAmount",
            "2026-03-01,S001,P001,2,100"
        });
        var wProducts = new[]
        {
            new ProductEntity { ProductId = "P001", ProductName = "Cola", UnitPrice = 120, Category = "Drink" }
        };

        Assert.That(
            () => FStore.ReadAndNormalizeSales(wPath, wProducts),
            Throws.TypeOf<DomainValidationException>());
    }

    [Test]
    /// <summary>
    /// 在庫データを書き込み後に再読込して同じ内容を取得できることを検証します。
    /// </summary>
    public void WriteAndReadInventories_WhenDataIsValid_RoundTrips()
    {
        var wPath = Path.Combine(FWorkDir, "inventory.csv");
        var wInput = new[]
        {
            new InventoryRecord { StoreId = "S001", ProductId = "P001", Stock = 3 }
        };

        FStore.WriteInventories(wPath, wInput);
        var wLoaded = FStore.ReadInventories(wPath);

        Assert.That(wLoaded.Count, Is.EqualTo(1));
        Assert.That(wLoaded[0].StoreId, Is.EqualTo("S001"));
        Assert.That(wLoaded[0].Stock, Is.EqualTo(3));
    }

    [Test]
    /// <summary>
    /// 在庫履歴データを書き込み後に再読込して同じ内容を取得できることを検証します。
    /// </summary>
    public void WriteAndReadInventoryHistories_WhenDataIsValid_RoundTrips()
    {
        var wPath = Path.Combine(FWorkDir, "inventory_history.csv");
        var wInput = new[]
        {
            new InventoryHistoryRecord
            {
                OccurredAt = new DateTime(2026, 3, 3, 10, 15, 0),
                OperationType = InventoryOperationTypeEnum.Inbound,
                StoreId = "S001",
                ProductId = "P001",
                Quantity = 5,
                ResultStock = 15,
                Result = "Success"
            }
        };

        FStore.WriteInventoryHistories(wPath, wInput);
        var wLoaded = FStore.ReadInventoryHistories(wPath);

        Assert.That(wLoaded.Count, Is.EqualTo(1));
        Assert.That(wLoaded[0].OccurredAt, Is.EqualTo(new DateTime(2026, 3, 3, 10, 15, 0)));
        Assert.That(wLoaded[0].OperationType, Is.EqualTo(InventoryOperationTypeEnum.Inbound));
        Assert.That(wLoaded[0].StoreId, Is.EqualTo("S001"));
        Assert.That(wLoaded[0].ProductId, Is.EqualTo("P001"));
        Assert.That(wLoaded[0].Quantity, Is.EqualTo(5));
        Assert.That(wLoaded[0].ResultStock, Is.EqualTo(15));
        Assert.That(wLoaded[0].Result, Is.EqualTo("Success"));
    }

    [Test]
    /// <summary>
    /// 既存ファイルに商品データを書き込む際にバックアップが作成されることを検証します。
    /// </summary>
    public void WriteProducts_WhenTargetExists_CreatesBackup()
    {
        var wPath = Path.Combine(FWorkDir, "products.csv");
        FStore.WriteProducts(wPath, new[]
        {
            new ProductEntity { ProductId = "P001", ProductName = "Cola", UnitPrice = 120, Category = "Drink" }
        });

        FStore.WriteProducts(wPath, new[]
        {
            new ProductEntity { ProductId = "P002", ProductName = "Tea", UnitPrice = 100, Category = "Drink" }
        });

        var wBackups = FStore.GetBackups(wPath);

        Assert.That(wBackups.Count, Is.GreaterThanOrEqualTo(1));
        Assert.That(File.Exists(wBackups[0]), Is.True);
    }

    [Test]
    /// <summary>
    /// バックアップが存在する場合に最新バックアップから元の内容へ復元できることを検証します。
    /// </summary>
    public void RestoreLatestBackup_WhenBackupsExist_RestoresPreviousContent()
    {
        var wPath = Path.Combine(FWorkDir, "products.csv");
        FStore.WriteProducts(wPath, new[]
        {
            new ProductEntity { ProductId = "P001", ProductName = "Cola", UnitPrice = 120, Category = "Drink" }
        });
        FStore.WriteProducts(wPath, new[]
        {
            new ProductEntity { ProductId = "P002", ProductName = "Tea", UnitPrice = 100, Category = "Drink" }
        });

        FStore.RestoreLatestBackup(wPath);
        var wRestored = FStore.ReadProducts(wPath);

        Assert.That(wRestored.Count, Is.EqualTo(1));
        Assert.That(wRestored[0].ProductId, Is.EqualTo("P001"));
    }

    [Test]
    /// <summary>
    /// 商品データを書き込んだ際に操作ログが出力されることを検証します。
    /// </summary>
    public void WriteProducts_WritesOperationLog()
    {
        var wPath = Path.Combine(FWorkDir, "products.csv");

        FStore.WriteProducts(wPath, new[]
        {
            new ProductEntity { ProductId = "P001", ProductName = "Cola", UnitPrice = 120, Category = "Drink" }
        });

        var wLogPath = Path.Combine(FWorkDir, "logs", "operations.log");
        Assert.That(File.Exists(wLogPath), Is.True);
        var wLog = File.ReadAllText(wLogPath);
        Assert.That(wLog, Does.Contain("Write succeeded"));
    }

    [Test]
    /// <summary>
    /// 商品CSVを非同期読み込みした場合に商品一覧を取得できることを検証します。
    /// </summary>
    public async Task ReadProductsAsync_WhenCsvIsValid_ReturnsProducts()
    {
        var wPath = Path.Combine(FWorkDir, "products.csv");
        File.WriteAllLines(wPath, new[]
        {
            "ProductId,ProductName,UnitPrice,Category",
            "P001,Cola,120,Drink"
        });

        var wResult = await FStore.ReadProductsAsync(wPath);

        Assert.That(wResult.Count, Is.EqualTo(1));
        Assert.That(wResult[0].ProductId, Is.EqualTo("P001"));
    }

    [Test]
    /// <summary>
    /// 商品CSVの非同期書き込みをキャンセルした場合にキャンセル例外が発生することを確認します。
    /// </summary>
    public void WriteProductsAsync_WhenCanceled_ThrowsOperationCanceledException()
    {
        var wPath = Path.Combine(FWorkDir, "products.csv");
        using var wCts = new CancellationTokenSource();
        wCts.Cancel();

        Assert.That(
            async () => await FStore.WriteProductsAsync(wPath, new[]
            {
                new ProductEntity { ProductId = "P001", ProductName = "Cola", UnitPrice = 120, Category = "Drink" }
            }, wCts.Token),
            Throws.InstanceOf<OperationCanceledException>());
    }
}

