using System;
using NUnit.Framework;
using SalesManagementApp.Core.Application.Exceptions;
using SalesManagementApp.Core.Application.Validation;

namespace SalesManagementApp.Tests.Common;

/// <summary>
/// ValidationGuard の仕様を検証するNUnitテストクラスです。
/// </summary>
public class ValidationGuardTests
{
    [Test]
    /// <summary>
    /// 必須項目に空白文字列を指定した場合に検証例外が発生することを確認します。
    /// </summary>
    public void RequireNotEmpty_WhenValueIsBlank_ThrowsDomainValidationException()
    {
        Assert.That(
            () => ValidationGuard.RequireNotEmpty(" ", "商品ID"),
            Throws.TypeOf<DomainValidationException>());
    }

    [Test]
    /// <summary>
    /// 正数必須の値に0を指定した場合に検証例外が発生することを確認します。
    /// </summary>
    public void RequirePositive_WhenValueIsZero_ThrowsDomainValidationException()
    {
        Assert.That(
            () => ValidationGuard.RequirePositive(0, "数量"),
            Throws.TypeOf<DomainValidationException>());
    }

    [Test]
    /// <summary>
    /// 開始日が終了日より後の期間を指定した場合に検証例外が発生することを確認します。
    /// </summary>
    public void RequireDateRange_WhenStartDateAfterEndDate_ThrowsDomainValidationException()
    {
        Assert.That(
            () => ValidationGuard.RequireDateRange(new DateTime(2026, 3, 10), new DateTime(2026, 3, 1)),
            Throws.TypeOf<DomainValidationException>());
    }
}
