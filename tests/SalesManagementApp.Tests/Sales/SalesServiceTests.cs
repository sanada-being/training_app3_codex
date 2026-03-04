using System;
using System.Collections.Generic;
using NUnit.Framework;
using SalesManagementApp.Core.Application.Exceptions;
using SalesManagementApp.Core.Application.Services;
using SalesManagementApp.Core.Domain.Entities;
using SalesManagementApp.Tests.TestHelpers;
using CoreProduct = SalesManagementApp.Core.Domain.Entities.Product;

namespace SalesManagementApp.Tests.Sales;

/// <summary>
/// SalesService の仕様を検証するNUnitテストクラスです。
/// </summary>
public class SalesServiceTests
{
    private SalesService FService = null!;
    private List<SaleRecord> FSales = null!;
    private List<CoreProduct> FProducts = null!;
    private List<InventoryRecord> FInventories = null!;

    [SetUp]
    /// <summary>
    /// 各テストの実行前にテストデータと依存オブジェクトを初期化します。
    /// </summary>
    public void SetUp()
    {
        FService = new SalesService();
        FSales = new List<SaleRecord>();
        FProducts = new List<CoreProduct>
        {
            new ProductBuilder().WithId("P001").WithName("Cola").WithPrice(120).WithCategory("Drink").Build()
        };
        FInventories = new List<InventoryRecord>
        {
            new InventoryRecordBuilder().WithStoreId("S001").WithProductId("P001").WithStock(10).Build()
        };
    }

    [Test]
    /// <summary>
    /// 正常な売上を登録した場合に売上追加と在庫減算が同時に行われることを検証します。
    /// </summary>
    public void RegisterSale_WhenInputIsValid_AddsSaleAndReducesInventory()
    {
        var input = new SaleRecordBuilder().WithDate(new DateTime(2026, 3, 1)).WithStoreId("S001").WithProductId("P001").WithQuantity(3).Build();

        var registered = FService.RegisterSale(FSales, FProducts, FInventories, input);

        Assert.That(FSales.Count, Is.EqualTo(1));
        Assert.That(registered.SalesAmount, Is.EqualTo(360));
        Assert.That(FInventories[0].Stock, Is.EqualTo(7));
    }

    [Test]
    /// <summary>
    /// 売上数量が在庫数を超える場合に検証例外が発生しデータが変更されないことを確認します。
    /// </summary>
    public void RegisterSale_WhenStockIsInsufficient_ThrowsValidationException()
    {
        var input = new SaleRecordBuilder().WithStoreId("S001").WithProductId("P001").WithQuantity(11).Build();

        Assert.That(() => FService.RegisterSale(FSales, FProducts, FInventories, input), Throws.TypeOf<DomainValidationException>());
        Assert.That(FSales.Count, Is.EqualTo(0));
        Assert.That(FInventories[0].Stock, Is.EqualTo(10));
    }

    [Test]
    /// <summary>
    /// 商品マスタに存在しない商品IDで売上登録した場合に検証例外が発生することを確認します。
    /// </summary>
    public void RegisterSale_WhenProductNotFound_ThrowsValidationException()
    {
        var input = new SaleRecordBuilder().WithStoreId("S001").WithProductId("P999").WithQuantity(1).Build();

        Assert.That(() => FService.RegisterSale(FSales, FProducts, FInventories, input), Throws.TypeOf<DomainValidationException>());
    }

    [Test]
    /// <summary>
    /// 売上数量が正数でない場合に検証例外が発生することを確認します。
    /// </summary>
    public void RegisterSale_WhenQuantityIsNotPositive_ThrowsValidationException()
    {
        var input = new SaleRecordBuilder().WithStoreId("S001").WithProductId("P001").WithQuantity(0).Build();

        Assert.That(() => FService.RegisterSale(FSales, FProducts, FInventories, input), Throws.TypeOf<DomainValidationException>());
    }

    [Test]
    /// <summary>
    /// 履歴コレクションを指定した売上登録で売上操作履歴が記録されることを検証します。
    /// </summary>
    public void RegisterSale_WhenHistoryCollectionIsProvided_RecordsSaleHistory()
    {
        var input = new SaleRecordBuilder().WithDate(new DateTime(2026, 3, 1)).WithStoreId("S001").WithProductId("P001").WithQuantity(2).Build();
        var histories = new List<InventoryHistoryRecord>();
        var occurredAt = new DateTime(2026, 3, 1, 12, 0, 0);

        FService.RegisterSale(FSales, FProducts, FInventories, input, histories, occurredAt);

        Assert.That(histories.Count, Is.EqualTo(1));
        Assert.That(histories[0].OccurredAt, Is.EqualTo(occurredAt));
        Assert.That(histories[0].OperationType, Is.EqualTo(InventoryOperationType.Sale));
        Assert.That(histories[0].StoreId, Is.EqualTo("S001"));
        Assert.That(histories[0].ProductId, Is.EqualTo("P001"));
        Assert.That(histories[0].Quantity, Is.EqualTo(2));
        Assert.That(histories[0].ResultStock, Is.EqualTo(8));
        Assert.That(histories[0].Result, Is.EqualTo("Success"));
    }

    [Test]
    /// <summary>
    /// 日付範囲と店舗条件に一致する売上だけを絞り込めることを検証します。
    /// </summary>
    public void GetFiltered_WhenDateAndStoreFilterSpecified_ReturnsMatchedSales()
    {
        FSales.Add(new SaleRecordBuilder().WithDate(new DateTime(2026, 3, 1)).WithStoreId("S001").WithProductId("P001").WithQuantity(1).WithSalesAmount(120).Build());
        FSales.Add(new SaleRecordBuilder().WithDate(new DateTime(2026, 3, 2)).WithStoreId("S002").WithProductId("P001").WithQuantity(1).WithSalesAmount(120).Build());
        FSales.Add(new SaleRecordBuilder().WithDate(new DateTime(2026, 3, 3)).WithStoreId("S001").WithProductId("P002").WithQuantity(1).WithSalesAmount(140).Build());

        var filtered = FService.GetFiltered(
            FSales,
            new DateTime(2026, 3, 1),
            new DateTime(2026, 3, 2),
            "S001",
            string.Empty);

        Assert.That(filtered.Count, Is.EqualTo(1));
        Assert.That(filtered[0].StoreId, Is.EqualTo("S001"));
        Assert.That(filtered[0].SaleDate, Is.EqualTo(new DateTime(2026, 3, 1)));
    }
}
