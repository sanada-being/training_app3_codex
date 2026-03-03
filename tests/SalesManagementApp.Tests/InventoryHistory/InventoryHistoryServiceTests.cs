using System;
using System.Collections.Generic;
using NUnit.Framework;
using SalesManagementApp.Core.Application.Services;
using SalesManagementApp.Core.Domain.Entities;

namespace SalesManagementApp.Tests.InventoryHistory;

public class InventoryHistoryServiceTests
{
    private InventoryHistoryService _service = null!;
    private List<InventoryHistoryRecord> _histories = null!;

    [SetUp]
    public void SetUp()
    {
        _service = new InventoryHistoryService();
        _histories = new List<InventoryHistoryRecord>
        {
            new InventoryHistoryRecord
            {
                OccurredAt = new DateTime(2026, 3, 1, 10, 0, 0),
                OperationType = InventoryOperationType.Inbound,
                StoreId = "S001",
                ProductId = "P001",
                Quantity = 10,
                ResultStock = 10,
                Result = "Success"
            },
            new InventoryHistoryRecord
            {
                OccurredAt = new DateTime(2026, 3, 2, 9, 0, 0),
                OperationType = InventoryOperationType.Sale,
                StoreId = "S001",
                ProductId = "P001",
                Quantity = 2,
                ResultStock = 8,
                Result = "Success"
            },
            new InventoryHistoryRecord
            {
                OccurredAt = new DateTime(2026, 3, 2, 12, 0, 0),
                OperationType = InventoryOperationType.Outbound,
                StoreId = "S002",
                ProductId = "P002",
                Quantity = 1,
                ResultStock = 4,
                Result = "Success"
            }
        };
    }

    [Test]
    public void Record_WhenInputIsValid_AppendsHistory()
    {
        _service.Record(
            _histories,
            new DateTime(2026, 3, 3, 10, 0, 0),
            InventoryOperationType.Inbound,
            "S001",
            "P003",
            5,
            12,
            "Success");

        Assert.That(_histories.Count, Is.EqualTo(4));
        Assert.That(_histories[3].OperationType, Is.EqualTo(InventoryOperationType.Inbound));
        Assert.That(_histories[3].ProductId, Is.EqualTo("P003"));
    }

    [Test]
    public void Filter_WhenConditionsProvided_ReturnsMatchedRecords()
    {
        var filtered = _service.Filter(
            _histories,
            new DateTime(2026, 3, 2, 0, 0, 0),
            new DateTime(2026, 3, 2, 23, 59, 59),
            "S001",
            "P001",
            InventoryOperationType.Sale);

        Assert.That(filtered.Count, Is.EqualTo(1));
        Assert.That(filtered[0].OperationType, Is.EqualTo(InventoryOperationType.Sale));
        Assert.That(filtered[0].StoreId, Is.EqualTo("S001"));
        Assert.That(filtered[0].ProductId, Is.EqualTo("P001"));
    }
}
