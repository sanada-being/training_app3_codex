using System;
using System.Collections.Generic;
using System.Linq;
using SalesManagementApp.Core.Application.Exceptions;
using SalesManagementApp.Core.Application.Models;
using SalesManagementApp.Core.Domain.Entities;

namespace SalesManagementApp.Core.Application.Services;

public class SalesAggregationService
{
    public IReadOnlyList<SaleRecord> FilterByPeriod(
        IReadOnlyCollection<SaleRecord> sales,
        DateTime startDate,
        DateTime endDate)
    {
        ValidatePeriod(startDate, endDate);

        var start = startDate.Date;
        var end = endDate.Date;

        return sales
            .Where(s => s.SaleDate.Date >= start && s.SaleDate.Date <= end)
            .OrderBy(s => s.SaleDate)
            .ThenBy(s => s.StoreId)
            .ThenBy(s => s.ProductId)
            .ToList();
    }

    public IReadOnlyList<ProductSalesSummary> GetProductSummaries(
        IReadOnlyCollection<SaleRecord> sales,
        DateTime startDate,
        DateTime endDate)
    {
        return FilterByPeriod(sales, startDate, endDate)
            .GroupBy(s => s.ProductId)
            .Select(g => new ProductSalesSummary
            {
                ProductId = g.Key,
                TotalQuantity = g.Sum(x => x.Quantity),
                TotalSalesAmount = g.Sum(x => x.SalesAmount)
            })
            .OrderBy(x => x.ProductId)
            .ToList();
    }

    public IReadOnlyList<WeeklySalesSummary> GetWeeklySummaries(
        IReadOnlyCollection<SaleRecord> sales,
        DateTime startDate,
        DateTime endDate)
    {
        return FilterByPeriod(sales, startDate, endDate)
            .GroupBy(s => GetWeekStartDate(s.SaleDate.Date))
            .Select(g =>
            {
                var weekStart = g.Key;
                return new WeeklySalesSummary
                {
                    WeekStartDate = weekStart,
                    WeekEndDate = weekStart.AddDays(6),
                    TotalQuantity = g.Sum(x => x.Quantity),
                    TotalSalesAmount = g.Sum(x => x.SalesAmount)
                };
            })
            .OrderBy(x => x.WeekStartDate)
            .ToList();
    }

    public int GetTotalSalesAmount(
        IReadOnlyCollection<SaleRecord> sales,
        DateTime startDate,
        DateTime endDate)
    {
        return FilterByPeriod(sales, startDate, endDate).Sum(s => s.SalesAmount);
    }

    private static void ValidatePeriod(DateTime startDate, DateTime endDate)
    {
        if (startDate.Date > endDate.Date)
        {
            throw new DomainValidationException("開始日は終了日以前で指定してください。");
        }
    }

    private static DateTime GetWeekStartDate(DateTime date)
    {
        const DayOfWeek day = DayOfWeek.Monday;
        var diff = (7 + (date.DayOfWeek - day)) % 7;
        return date.AddDays(-diff);
    }
}
