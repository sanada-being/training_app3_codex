using System;
using SalesManagementApp.Core.Application.Exceptions;
using SalesManagementApp.Core.Application.Validation;

namespace SalesManagementApp.Core.Application.Services;

/// <summary>
/// InventoryStockCalculator クラスです。
/// </summary>
public class InventoryStockCalculator
{
    /// <summary>
    /// 公開メソッドです。
    /// </summary>
    public int CalculateAfterInbound(int currentStock, int inboundQuantity)
    {
        ValidationGuard.RequireNonNegative(currentStock, "在庫数");
        ValidationGuard.RequirePositive(inboundQuantity, "入荷数量");

        try
        {
            return checked(currentStock + inboundQuantity);
        }
        catch (OverflowException)
        {
            throw new DomainValidationException("在庫算出中に上限を超えました。");
        }
    }

    /// <summary>
    /// 公開メソッドです。
    /// </summary>
    public int CalculateAfterOutbound(int currentStock, int outboundQuantity)
    {
        ValidationGuard.RequireNonNegative(currentStock, "在庫数");
        ValidationGuard.RequirePositive(outboundQuantity, "出庫数量");

        if (currentStock < outboundQuantity)
        {
            throw new DomainValidationException("在庫が不足しています。");
        }

        return currentStock - outboundQuantity;
    }
}
