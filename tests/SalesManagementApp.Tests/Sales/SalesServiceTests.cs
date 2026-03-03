using System;
using System.Collections.Generic;
using NUnit.Framework;
using SalesManagementApp.Core.Application.Exceptions;
using SalesManagementApp.Core.Application.Services;
using SalesManagementApp.Core.Domain.Entities;
using SalesManagementApp.Tests.TestHelpers;
using CoreProduct = SalesManagementApp.Core.Domain.Entities.Product;

namespace SalesManagementApp.Tests.Sales;

public class SalesServiceTests
{
    private SalesService _service = null!;
    private List<SaleRecord> _sales = null!;
    private List<CoreProduct> _products = null!;
    private List<InventoryRecord> _inventories = null!;

    [SetUp]
    public void SetUp()
    {
        _service = new SalesService();
        _sales = new List<SaleRecord>();
        _products = new List<CoreProduct>
        {
            new ProductBuilder().WithId("P001").WithName("Cola").WithPrice(120).WithCategory("Drink").Build()
        };
        _inventories = new List<InventoryRecord>
        {
            new InventoryRecordBuilder().WithStoreId("S001").WithProductId("P001").WithStock(10).Build()
        };
    }

    [Test]
    public void RegisterSale_WhenInputIsValid_AddsSaleAndReducesInventory()
    {
        var input = new SaleRecordBuilder().WithDate(new DateTime(2026, 3, 1)).WithStoreId("S001").WithProductId("P001").WithQuantity(3).Build();

        var registered = _service.RegisterSale(_sales, _products, _inventories, input);

        Assert.That(_sales.Count, Is.EqualTo(1));
        Assert.That(registered.SalesAmount, Is.EqualTo(360));
        Assert.That(_inventories[0].Stock, Is.EqualTo(7));
    }

    [Test]
    public void RegisterSale_WhenStockIsInsufficient_ThrowsValidationException()
    {
        var input = new SaleRecordBuilder().WithStoreId("S001").WithProductId("P001").WithQuantity(11).Build();

        Assert.That(() => _service.RegisterSale(_sales, _products, _inventories, input), Throws.TypeOf<DomainValidationException>());
        Assert.That(_sales.Count, Is.EqualTo(0));
        Assert.That(_inventories[0].Stock, Is.EqualTo(10));
    }

    [Test]
    public void RegisterSale_WhenProductNotFound_ThrowsValidationException()
    {
        var input = new SaleRecordBuilder().WithStoreId("S001").WithProductId("P999").WithQuantity(1).Build();

        Assert.That(() => _service.RegisterSale(_sales, _products, _inventories, input), Throws.TypeOf<DomainValidationException>());
    }

    [Test]
    public void RegisterSale_WhenQuantityIsNotPositive_ThrowsValidationException()
    {
        var input = new SaleRecordBuilder().WithStoreId("S001").WithProductId("P001").WithQuantity(0).Build();

        Assert.That(() => _service.RegisterSale(_sales, _products, _inventories, input), Throws.TypeOf<DomainValidationException>());
    }
}
