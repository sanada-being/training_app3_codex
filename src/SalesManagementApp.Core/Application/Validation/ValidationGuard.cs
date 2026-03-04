using System;
using SalesManagementApp.Core.Application.Exceptions;

namespace SalesManagementApp.Core.Application.Validation;

/// <summary>
/// ValidationGuard クラスです。
/// </summary>
public static class ValidationGuard
{
    /// <summary>
    /// 公開メソッドです。
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
    /// 公開メソッドです。
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
    /// 公開メソッドです。
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
    /// 公開メソッドです。
    /// </summary>
    public static void RequireDateRange(DateTime startDate, DateTime endDate)
    {
        if (startDate.Date > endDate.Date)
        {
            throw new DomainValidationException("開始日は終了日以前で指定してください。");
        }
    }
}
