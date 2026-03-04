using System;
using SalesManagementApp.Core.Application.Exceptions;
using SalesManagementApp.Core.Application.Validation;

namespace SalesManagementApp.Core.Application.Services;

/// <summary>
/// 在庫増減後の在庫数を計算し入力妥当性を検証します。
/// </summary>
public class InventoryStockCalculator
{
    /// <summary>
    /// 入庫後在庫を計算し、オーバーフローを検証します。
    /// </summary>
    public int CalculateAfterInbound(int vCurrentStock, int vInboundQuantity)
    {
        ValidationGuard.RequireNonNegative(vCurrentStock, "在庫数");
        ValidationGuard.RequirePositive(vInboundQuantity, "入荷数量");

        try
        {
            return checked(vCurrentStock + vInboundQuantity);
        }
        catch (OverflowException)
        {
            throw new DomainValidationException("在庫算出中に上限を超えました。");
        }
    }

    /// <summary>
    /// 出庫後在庫を計算し、在庫不足を検証します。
    /// </summary>
    public int CalculateAfterOutbound(int vCurrentStock, int vOutboundQuantity)
    {
        ValidationGuard.RequireNonNegative(vCurrentStock, "在庫数");
        ValidationGuard.RequirePositive(vOutboundQuantity, "出庫数量");

        if (vCurrentStock < vOutboundQuantity)
        {
            throw new DomainValidationException("在庫が不足しています。");
        }

        return vCurrentStock - vOutboundQuantity;
    }
}
