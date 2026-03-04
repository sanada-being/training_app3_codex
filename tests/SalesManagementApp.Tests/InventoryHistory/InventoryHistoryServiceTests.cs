using System;
using System.Collections.Generic;
using NUnit.Framework;
using SalesManagementApp.Core.Application.Services;
using SalesManagementApp.Core.Domain.Entities;

namespace SalesManagementApp.Tests.InventoryHistory;

/// <summary>
/// InventoryHistoryService の仕様を検証するNUnitテストクラスです。
/// </summary>
public class InventoryHistoryServiceTests
{
    private InventoryHistoryService FService = null!;
    private List<InventoryHistoryRecord> FHistories = null!;

    [SetUp]
    /// <summary>
    /// 公開メソッドです。
    /// </summary>
    public void SetUp()
    {
        FService = new InventoryHistoryService();
        FHistories = new List<InventoryHistoryRecord>
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
    /// <summary>
    /// 公開メソッドです。
    /// </summary>
    public void Record_WhenInputIsValid_AppendsHistory()
    {
        FService.Record(
            FHistories,
            new DateTime(2026, 3, 3, 10, 0, 0),
            InventoryOperationType.Inbound,
            "S001",
            "P003",
            5,
            12,
            "Success");

        Assert.That(FHistories.Count, Is.EqualTo(4));
        Assert.That(FHistories[3].OperationType, Is.EqualTo(InventoryOperationType.Inbound));
        Assert.That(FHistories[3].ProductId, Is.EqualTo("P003"));
    }

    [Test]
    /// <summary>
    /// 公開メソッドです。
    /// </summary>
    public void Filter_WhenConditionsProvided_ReturnsMatchedRecords()
    {
        var filtered = FService.Filter(
            FHistories,
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
