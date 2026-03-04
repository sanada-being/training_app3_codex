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
        IReadOnlyCollection<SaleRecord> vSales,
        DateTime vStartDate,
        DateTime vEndDate)
    {
        ValidatePeriod(vStartDate, vEndDate);

        var wStart = vStartDate.Date;
        var wEnd = vEndDate.Date;

        return vSales
            .Where(vS => vS.SaleDate.Date >= wStart && vS.SaleDate.Date <= wEnd)
            .OrderBy(vS => vS.SaleDate)
            .ThenBy(vS => vS.StoreId)
            .ThenBy(vS => vS.ProductId)
            .ToList();
    }

    /// <summary>
    /// 商品別の売上数量・売上金額サマリーを集計します。
    /// </summary>
    public IReadOnlyList<ProductSalesSummary> GetProductSummaries(
        IReadOnlyCollection<SaleRecord> vSales,
        DateTime vStartDate,
        DateTime vEndDate)
    {
        return FilterByPeriod(vSales, vStartDate, vEndDate)
            .GroupBy(vS => vS.ProductId)
            .Select(vG => new ProductSalesSummary
            {
                ProductId = vG.Key,
                TotalQuantity = vG.Sum(vX => vX.Quantity),
                TotalSalesAmount = vG.Sum(vX => vX.SalesAmount)
            })
            .OrderBy(vX => vX.ProductId)
            .ToList();
    }

    /// <summary>
    /// 週別の売上数量・売上金額サマリーを集計します。
    /// </summary>
    public IReadOnlyList<WeeklySalesSummary> GetWeeklySummaries(
        IReadOnlyCollection<SaleRecord> vSales,
        DateTime vStartDate,
        DateTime vEndDate)
    {
        return FilterByPeriod(vSales, vStartDate, vEndDate)
            .GroupBy(vS => GetWeekStartDate(vS.SaleDate.Date))
            .Select(vG =>
            {
                var wWeekStart = vG.Key;
                return new WeeklySalesSummary
                {
                    WeekStartDate = wWeekStart,
                    WeekEndDate = wWeekStart.AddDays(6),
                    TotalQuantity = vG.Sum(vX => vX.Quantity),
                    TotalSalesAmount = vG.Sum(vX => vX.SalesAmount)
                };
            })
            .OrderBy(vX => vX.WeekStartDate)
            .ToList();
    }

    /// <summary>
    /// 対象期間の総売上金額を算出します。
    /// </summary>
    public int GetTotalSalesAmount(
        IReadOnlyCollection<SaleRecord> vSales,
        DateTime vStartDate,
        DateTime vEndDate)
    {
        return FilterByPeriod(vSales, vStartDate, vEndDate).Sum(vS => vS.SalesAmount);
    }

    private static void ValidatePeriod(DateTime vStartDate, DateTime vEndDate)
    {
        ValidationGuard.RequireDateRange(vStartDate, vEndDate);
    }

    private static DateTime GetWeekStartDate(DateTime vDate)
    {
        const DayOfWeek wDay = DayOfWeek.Monday;
        var wDiff = (7 + (vDate.DayOfWeek - wDay)) % 7;
        return vDate.AddDays(-wDiff);
    }
}
