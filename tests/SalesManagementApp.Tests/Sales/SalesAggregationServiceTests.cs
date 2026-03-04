using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SalesManagementApp.Core.Application.Exceptions;
using SalesManagementApp.Core.Application.Services;
using SalesManagementApp.Tests.TestHelpers;

namespace SalesManagementApp.Tests.Sales;

/// <summary>
/// SalesAggregationService の仕様を検証するNUnitテストクラスです。
/// </summary>
public class SalesAggregationServiceTests
{
    private SalesAggregationService FService = null!;
    private List<SalesManagementApp.Core.Domain.Entities.SaleRecord> FSales = null!;

    [SetUp]
    /// <summary>
    /// 公開メソッドです。
    /// </summary>
    public void SetUp()
    {
        FService = new SalesAggregationService();
        FSales = new List<SalesManagementApp.Core.Domain.Entities.SaleRecord>
        {
            new SaleRecordBuilder().WithDate(new DateTime(2026, 3, 1)).WithStoreId("S001").WithProductId("P001").WithQuantity(2).WithSalesAmount(200).Build(),
            new SaleRecordBuilder().WithDate(new DateTime(2026, 3, 2)).WithStoreId("S001").WithProductId("P001").WithQuantity(1).WithSalesAmount(100).Build(),
            new SaleRecordBuilder().WithDate(new DateTime(2026, 3, 3)).WithStoreId("S001").WithProductId("P002").WithQuantity(3).WithSalesAmount(450).Build(),
            new SaleRecordBuilder().WithDate(new DateTime(2026, 3, 8)).WithStoreId("S002").WithProductId("P002").WithQuantity(2).WithSalesAmount(300).Build(),
            new SaleRecordBuilder().WithDate(new DateTime(2026, 3, 10)).WithStoreId("S001").WithProductId("P001").WithQuantity(1).WithSalesAmount(100).Build()
        };
    }

    [Test]
    /// <summary>
    /// 公開メソッドです。
    /// </summary>
    public void FilterByPeriod_IncludesBoundaryDates()
    {
        var result = FService.FilterByPeriod(FSales, new DateTime(2026, 3, 1), new DateTime(2026, 3, 8));

        Assert.That(result.Count, Is.EqualTo(4));
        Assert.That(result.First().SaleDate.Date, Is.EqualTo(new DateTime(2026, 3, 1)));
        Assert.That(result.Last().SaleDate.Date, Is.EqualTo(new DateTime(2026, 3, 8)));
    }

    [Test]
    /// <summary>
    /// 公開メソッドです。
    /// </summary>
    public void GetProductSummaries_ReturnsAmountAndQuantityByProduct()
    {
        var summaries = FService.GetProductSummaries(FSales, new DateTime(2026, 3, 1), new DateTime(2026, 3, 8));
        var p001 = summaries.Single(s => s.ProductId == "P001");
        var p002 = summaries.Single(s => s.ProductId == "P002");

        Assert.That(p001.TotalQuantity, Is.EqualTo(3));
        Assert.That(p001.TotalSalesAmount, Is.EqualTo(300));
        Assert.That(p002.TotalQuantity, Is.EqualTo(5));
        Assert.That(p002.TotalSalesAmount, Is.EqualTo(750));
    }

    [Test]
    /// <summary>
    /// 公開メソッドです。
    /// </summary>
    public void GetWeeklySummaries_ReturnsWeeklyTotals()
    {
        var summaries = FService.GetWeeklySummaries(FSales, new DateTime(2026, 3, 1), new DateTime(2026, 3, 8));

        Assert.That(summaries.Count, Is.EqualTo(2));
        Assert.That(summaries[0].TotalSalesAmount, Is.EqualTo(200));
        Assert.That(summaries[1].TotalSalesAmount, Is.EqualTo(850));
    }

    [Test]
    /// <summary>
    /// 公開メソッドです。
    /// </summary>
    public void GetTotalSalesAmount_ReturnsPeriodAmount()
    {
        var total = FService.GetTotalSalesAmount(FSales, new DateTime(2026, 3, 1), new DateTime(2026, 3, 8));

        Assert.That(total, Is.EqualTo(1050));
    }

    [Test]
    /// <summary>
    /// 公開メソッドです。
    /// </summary>
    public void FilterByPeriod_WhenStartDateIsAfterEndDate_ThrowsValidationException()
    {
        Assert.That(
            () => FService.FilterByPeriod(FSales, new DateTime(2026, 3, 9), new DateTime(2026, 3, 8)),
            Throws.TypeOf<DomainValidationException>());
    }
}
