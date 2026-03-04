using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using SalesManagementApp.Core.Application.Exceptions;
using SalesManagementApp.Core.Application.Services;
using SalesManagementApp.Core.Domain.Entities;
using SalesManagementApp.Tests.TestHelpers;

namespace SalesManagementApp.Tests.Inventory;

/// <summary>
/// InventoryService の仕様を検証するNUnitテストクラスです。
/// </summary>
public class InventoryServiceTests
{
    private InventoryService FService = null!;
    private List<SalesManagementApp.Core.Domain.Entities.InventoryRecord> FRecords = null!;

    [SetUp]
    /// <summary>
    /// 各テストの実行前にテストデータと依存オブジェクトを初期化します。
    /// </summary>
    public void SetUp()
    {
        FService = new InventoryService();
        FRecords = new List<SalesManagementApp.Core.Domain.Entities.InventoryRecord>();
    }

    [Test]
    /// <summary>
    /// 在庫レコードが存在しない場合に新規作成して在庫を加算できることを検証します。
    /// </summary>
    public void AddStock_WhenRecordNotExists_CreatesAndAddsStock()
    {
        FService.AddStock(FRecords, "S001", "P001", 3);

        Assert.That(FRecords.Count, Is.EqualTo(1));
        Assert.That(FRecords[0].Stock, Is.EqualTo(3));
    }

    [Test]
    /// <summary>
    /// 出庫数量が現在在庫を超える場合に検証例外が発生することを確認します。
    /// </summary>
    public void RemoveStock_WhenStockIsInsufficient_ThrowsValidationException()
    {
        FRecords.Add(new InventoryRecordBuilder().WithStoreId("S001").WithProductId("P001").WithStock(2).Build());

        Assert.That(() => FService.RemoveStock(FRecords, "S001", "P001", 3), Throws.TypeOf<DomainValidationException>());
    }

    [Test]
    /// <summary>
    /// 在庫数が閾値以下の商品だけを発注対象として取得できることを検証します。
    /// </summary>
    public void GetReorderTargets_WhenStockIsLessThanOrEqualToThreshold_ReturnsTargets()
    {
        FRecords.Add(new InventoryRecordBuilder().WithStoreId("S001").WithProductId("P001").WithStock(5).Build());
        FRecords.Add(new InventoryRecordBuilder().WithStoreId("S001").WithProductId("P002").WithStock(6).Build());

        var targets = FService.GetReorderTargets(FRecords, 5);

        Assert.That(targets.Count, Is.EqualTo(1));
        Assert.That(targets[0].ProductId, Is.EqualTo("P001"));
    }

    [Test]
    /// <summary>
    /// 在庫加算が同時実行された場合でもデータ欠損なく在庫を更新できることを検証します。
    /// </summary>
    public void AddStock_WhenConcurrentRequestsOccur_UpdatesWithoutDataLoss()
    {
        FRecords.Add(new InventoryRecordBuilder().WithStoreId("S001").WithProductId("P001").WithStock(0).Build());

        var tasks = Enumerable.Range(0, 30)
            .Select(_ => Task.Run(() => FService.AddStock(FRecords, "S001", "P001", 1)))
            .ToArray();
        Task.WaitAll(tasks);

        Assert.That(FRecords[0].Stock, Is.EqualTo(30));
    }

    [Test]
    /// <summary>
    /// 在庫加算結果がオーバーフローする場合に検証例外が発生することを確認します。
    /// </summary>
    public void AddStock_WhenStockCalculationOverflows_ThrowsValidationException()
    {
        FRecords.Add(new InventoryRecordBuilder().WithStoreId("S001").WithProductId("P001").WithStock(int.MaxValue).Build());

        Assert.That(
            () => FService.AddStock(FRecords, "S001", "P001", 1),
            Throws.TypeOf<DomainValidationException>());
    }

    [Test]
    /// <summary>
    /// 履歴コレクションを指定した在庫加算で入庫履歴が記録されることを検証します。
    /// </summary>
    public void AddStock_WhenHistoryCollectionIsProvided_RecordsInboundHistory()
    {
        var histories = new List<InventoryHistoryRecord>();
        var occurredAt = new DateTime(2026, 3, 3, 9, 30, 0);

        FService.AddStock(FRecords, "S001", "P001", 3, histories, occurredAt);

        Assert.That(histories.Count, Is.EqualTo(1));
        Assert.That(histories[0].OccurredAt, Is.EqualTo(occurredAt));
        Assert.That(histories[0].OperationType, Is.EqualTo(InventoryOperationType.Inbound));
        Assert.That(histories[0].StoreId, Is.EqualTo("S001"));
        Assert.That(histories[0].ProductId, Is.EqualTo("P001"));
        Assert.That(histories[0].Quantity, Is.EqualTo(3));
        Assert.That(histories[0].ResultStock, Is.EqualTo(3));
        Assert.That(histories[0].Result, Is.EqualTo("Success"));
    }

    [Test]
    /// <summary>
    /// 店舗IDと商品IDの条件で在庫レコードを絞り込めることを検証します。
    /// </summary>
    public void GetFiltered_WhenStoreAndProductFiltersSpecified_ReturnsMatchedRecords()
    {
        FRecords.Add(new InventoryRecordBuilder().WithStoreId("S001").WithProductId("P001").WithStock(2).Build());
        FRecords.Add(new InventoryRecordBuilder().WithStoreId("S001").WithProductId("P002").WithStock(3).Build());
        FRecords.Add(new InventoryRecordBuilder().WithStoreId("S002").WithProductId("P001").WithStock(4).Build());

        var filtered = FService.GetFiltered(FRecords, "S001", "P002");

        Assert.That(filtered.Count, Is.EqualTo(1));
        Assert.That(filtered[0].StoreId, Is.EqualTo("S001"));
        Assert.That(filtered[0].ProductId, Is.EqualTo("P002"));
    }
}
