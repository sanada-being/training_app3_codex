using System;
using System.IO;
using NUnit.Framework;
using SalesManagementApp.Core.Application.State;
using SalesManagementApp.Core.Domain.Entities;
using SalesManagementApp.Core.Infrastructure.Csv;
using ProductEntity = SalesManagementApp.Core.Domain.Entities.Product;

namespace SalesManagementApp.Tests.AppState;

/// <summary>
/// AppDataRepository の仕様を検証するNUnitテストクラスです。
/// </summary>
public class AppDataRepositoryTests
{
    private string FWorkDir = string.Empty;
    private AppDataRepository FRepository = null!;

    [SetUp]
    /// <summary>
    /// 各テストの実行前にテストデータと依存オブジェクトを初期化します。
    /// </summary>
    public void SetUp()
    {
        FWorkDir = Path.Combine(Path.GetTempPath(), "SalesManagementApp.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(FWorkDir);
        FRepository = new AppDataRepository();
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
    /// 商品CSVが存在しない場合に空の一覧を返すことを検証します。
    /// </summary>
    public void ReadProductsIfExists_WhenFileDoesNotExist_ReturnsEmpty()
    {
        var wPath = Path.Combine(FWorkDir, "products.csv");

        var wResult = FRepository.ReadProductsIfExists(wPath);

        Assert.That(wResult, Is.Empty);
    }

    [Test]
    /// <summary>
    /// 売上CSVが旧ヘッダー形式の場合に売上金額を補完し、CSVを正規化できることを検証します。
    /// </summary>
    public void ReadAndNormalizeSalesIfExists_WhenLegacyHeader_ReturnsSalesAndNormalizesCsv()
    {
        var wPath = Path.Combine(FWorkDir, "sales.csv");
        File.WriteAllLines(wPath, new[]
        {
            "SaleDate,StoreId,ProductId,Quantity",
            "2026-03-03,S001,P001,2"
        });
        var wProducts = new[]
        {
            new ProductEntity { ProductId = "P001", ProductName = "Cola", UnitPrice = 120, Category = "Drink" }
        };

        var wResult = FRepository.ReadAndNormalizeSalesIfExists(wPath, wProducts);

        Assert.That(wResult.Count, Is.EqualTo(1));
        Assert.That(wResult[0].SalesAmount, Is.EqualTo(240));
        var wLines = File.ReadAllLines(wPath);
        Assert.That(wLines[0], Is.EqualTo("SaleDate,StoreId,ProductId,Quantity,SalesAmount"));
    }

    [Test]
    /// <summary>
    /// 在庫履歴を書き込んだ後に再読込して、値が保持されることを検証します。
    /// </summary>
    public void WriteInventoryHistories_WhenPathIsProvided_WritesCsv()
    {
        var wPath = Path.Combine(FWorkDir, "inventory_history.csv");
        var wRecords = new[]
        {
            new InventoryHistoryRecord
            {
                OccurredAt = new DateTime(2026, 3, 3, 8, 0, 0),
                OperationType = InventoryOperationTypeEnum.Inbound,
                StoreId = "S001",
                ProductId = "P001",
                Quantity = 3,
                ResultStock = 13,
                Result = "Success"
            }
        };

        FRepository.WriteInventoryHistories(wPath, wRecords);

        var wReloaded = new CsvDataStore().ReadInventoryHistories(wPath);
        Assert.That(wReloaded.Count, Is.EqualTo(1));
        Assert.That(wReloaded[0].OperationType, Is.EqualTo(InventoryOperationTypeEnum.Inbound));
        Assert.That(wReloaded[0].ResultStock, Is.EqualTo(13));
    }
}

