using System;
using System.Collections.Generic;
using System.Linq;
using SalesManagementApp.Core.Application.Models;
using SalesManagementApp.Core.Application.Validation;
using SalesManagementApp.Core.Domain.Entities;

namespace SalesManagementApp.Core.Application.Services;

/// <summary>
/// 売上データの期間集計を提供するアプリケーションサービスです。
/// </summary>
public class SalesAggregationService
{
    /// <summary>
    /// 指定期間内の売上データを抽出します。
    /// </summary>
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

    /// <summary>
    /// 商品別の売上数量・売上金額サマリーを集計します。
    /// </summary>
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

    /// <summary>
    /// 週別の売上数量・売上金額サマリーを集計します。
    /// </summary>
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

    /// <summary>
    /// 対象期間の総売上金額を算出します。
    /// </summary>
    public int GetTotalSalesAmount(
        IReadOnlyCollection<SaleRecord> sales,
        DateTime startDate,
        DateTime endDate)
    {
        return FilterByPeriod(sales, startDate, endDate).Sum(s => s.SalesAmount);
    }

    private static void ValidatePeriod(DateTime startDate, DateTime endDate)
    {
        ValidationGuard.RequireDateRange(startDate, endDate);
    }

    private static DateTime GetWeekStartDate(DateTime date)
    {
        const DayOfWeek day = DayOfWeek.Monday;
        var diff = (7 + (date.DayOfWeek - day)) % 7;
        return date.AddDays(-diff);
    }
}
