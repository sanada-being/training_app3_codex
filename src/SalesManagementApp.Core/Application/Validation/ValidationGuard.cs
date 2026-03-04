using System;
using SalesManagementApp.Core.Application.Exceptions;

namespace SalesManagementApp.Core.Application.Validation;

/// <summary>
/// 入力値検証の共通ガードメソッドを提供します。
/// </summary>
public static class ValidationGuard
{
    /// <summary>
    /// 文字列が必須入力条件を満たすことを検証します。
    /// </summary>
    public static string RequireNotEmpty(string? vValue, string vFieldName)
    {
        if (string.IsNullOrWhiteSpace(vValue))
        {
            throw new DomainValidationException($"{vFieldName} は必須です。");
        }

        return vValue!.Trim();
    }

    /// <summary>
    /// 数値が正数であることを検証します。
    /// </summary>
    public static int RequirePositive(int vValue, string vFieldName)
    {
        if (vValue <= 0)
        {
            throw new DomainValidationException($"{vFieldName} は1以上で入力してください。");
        }

        return vValue;
    }

    /// <summary>
    /// 数値が0以上であることを検証します。
    /// </summary>
    public static int RequireNonNegative(int vValue, string vFieldName)
    {
        if (vValue < 0)
        {
            throw new DomainValidationException($"{vFieldName} は0以上で入力してください。");
        }

        return vValue;
    }

    /// <summary>
    /// 開始日が終了日以前であることを検証します。
    /// </summary>
    public static void RequireDateRange(DateTime vStartDate, DateTime vEndDate)
    {
        if (vStartDate.Date > vEndDate.Date)
        {
            throw new DomainValidationException("開始日は終了日以前で指定してください。");
        }
    }
}
