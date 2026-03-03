using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using SalesManagementApp.Core.Application.Exceptions;
using SalesManagementApp.Core.Application.Services;
using SalesManagementApp.Core.Domain.Entities;
using SalesManagementApp.Tests.TestHelpers;

namespace SalesManagementApp.Tests.Inventory;

public class InventoryServiceTests
{
    private InventoryService _service = null!;
    private List<SalesManagementApp.Core.Domain.Entities.InventoryRecord> _records = null!;

    [SetUp]
    public void SetUp()
    {
        _service = new InventoryService();
        _records = new List<SalesManagementApp.Core.Domain.Entities.InventoryRecord>();
    }

    [Test]
    public void AddStock_WhenRecordNotExists_CreatesAndAddsStock()
    {
        _service.AddStock(_records, "S001", "P001", 3);

        Assert.That(_records.Count, Is.EqualTo(1));
        Assert.That(_records[0].Stock, Is.EqualTo(3));
    }

    [Test]
    public void RemoveStock_WhenStockIsInsufficient_ThrowsValidationException()
    {
        _records.Add(new InventoryRecordBuilder().WithStoreId("S001").WithProductId("P001").WithStock(2).Build());

        Assert.That(() => _service.RemoveStock(_records, "S001", "P001", 3), Throws.TypeOf<DomainValidationException>());
    }

    [Test]
    public void GetReorderTargets_WhenStockIsLessThanOrEqualToThreshold_ReturnsTargets()
    {
        _records.Add(new InventoryRecordBuilder().WithStoreId("S001").WithProductId("P001").WithStock(5).Build());
        _records.Add(new InventoryRecordBuilder().WithStoreId("S001").WithProductId("P002").WithStock(6).Build());

        var targets = _service.GetReorderTargets(_records, 5);

        Assert.That(targets.Count, Is.EqualTo(1));
        Assert.That(targets[0].ProductId, Is.EqualTo("P001"));
    }

    [Test]
    public void AddStock_WhenConcurrentRequestsOccur_UpdatesWithoutDataLoss()
    {
        _records.Add(new InventoryRecordBuilder().WithStoreId("S001").WithProductId("P001").WithStock(0).Build());

        var tasks = Enumerable.Range(0, 30)
            .Select(_ => Task.Run(() => _service.AddStock(_records, "S001", "P001", 1)))
            .ToArray();
        Task.WaitAll(tasks);

        Assert.That(_records[0].Stock, Is.EqualTo(30));
    }

    [Test]
    public void AddStock_WhenStockCalculationOverflows_ThrowsValidationException()
    {
        _records.Add(new InventoryRecordBuilder().WithStoreId("S001").WithProductId("P001").WithStock(int.MaxValue).Build());

        Assert.That(
            () => _service.AddStock(_records, "S001", "P001", 1),
            Throws.TypeOf<DomainValidationException>());
    }

    [Test]
    public void AddStock_WhenHistoryCollectionIsProvided_RecordsInboundHistory()
    {
        var histories = new List<InventoryHistoryRecord>();
        var occurredAt = new DateTime(2026, 3, 3, 9, 30, 0);

        _service.AddStock(_records, "S001", "P001", 3, histories, occurredAt);

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
    public void GetFiltered_WhenStoreAndProductFiltersSpecified_ReturnsMatchedRecords()
    {
        _records.Add(new InventoryRecordBuilder().WithStoreId("S001").WithProductId("P001").WithStock(2).Build());
        _records.Add(new InventoryRecordBuilder().WithStoreId("S001").WithProductId("P002").WithStock(3).Build());
        _records.Add(new InventoryRecordBuilder().WithStoreId("S002").WithProductId("P001").WithStock(4).Build());

        var filtered = _service.GetFiltered(_records, "S001", "P002");

        Assert.That(filtered.Count, Is.EqualTo(1));
        Assert.That(filtered[0].StoreId, Is.EqualTo("S001"));
        Assert.That(filtered[0].ProductId, Is.EqualTo("P002"));
    }
}
