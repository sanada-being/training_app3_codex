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
    public static string RequireNotEmpty(string? value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainValidationException($"{fieldName} は必須です。");
        }

        return value!.Trim();
    }

    /// <summary>
    /// 数値が正数であることを検証します。
    /// </summary>
    public static int RequirePositive(int value, string fieldName)
    {
        if (value <= 0)
        {
            throw new DomainValidationException($"{fieldName} は1以上で入力してください。");
        }

        return value;
    }

    /// <summary>
    /// 数値が0以上であることを検証します。
    /// </summary>
    public static int RequireNonNegative(int value, string fieldName)
    {
        if (value < 0)
        {
            throw new DomainValidationException($"{fieldName} は0以上で入力してください。");
        }

        return value;
    }

    /// <summary>
    /// 開始日が終了日以前であることを検証します。
    /// </summary>
    public static void RequireDateRange(DateTime startDate, DateTime endDate)
    {
        if (startDate.Date > endDate.Date)
        {
            throw new DomainValidationException("開始日は終了日以前で指定してください。");
        }
    }
}
