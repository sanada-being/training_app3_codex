using System;
using NUnit.Framework;
using SalesManagementApp.Core.Application.Errors;
using SalesManagementApp.Core.Application.Exceptions;

namespace SalesManagementApp.Tests.Common;

/// <summary>
/// ErrorHandlingPolicy の仕様を検証するNUnitテストクラスです。
/// </summary>
public class ErrorHandlingPolicyTests
{
    [Test]
    /// <summary>
    /// 入力検証例外を渡した場合に、入力エラー向けの表示情報へ変換されることを検証します。
    /// </summary>
    public void CreatePresentation_WhenDomainValidationException_ReturnsValidationCategory()
    {
        var wEx = new DomainValidationException("入力不備です。");

        var wResult = ErrorHandlingPolicy.CreatePresentation(wEx);

        Assert.That(wResult.Category, Is.EqualTo(ErrorCategoryEnum.Validation));
        Assert.That(wResult.Title, Is.EqualTo("入力エラー"));
        Assert.That(wResult.UserMessage, Is.EqualTo("入力不備です。"));
        Assert.That(wResult.LogLevel, Is.EqualTo("WARN"));
    }

    [Test]
    /// <summary>
    /// 業務処理例外を渡した場合に、業務エラー向けの表示情報へ変換されることを検証します。
    /// </summary>
    public void CreatePresentation_WhenApplicationOperationException_ReturnsOperationCategory()
    {
        var wEx = new ApplicationOperationException("業務処理に失敗しました。");

        var wResult = ErrorHandlingPolicy.CreatePresentation(wEx);

        Assert.That(wResult.Category, Is.EqualTo(ErrorCategoryEnum.Operation));
        Assert.That(wResult.Title, Is.EqualTo("業務エラー"));
        Assert.That(wResult.LogLevel, Is.EqualTo("ERROR"));
    }

    [Test]
    /// <summary>
    /// 想定外の例外を渡した場合に、汎用的なシステムエラー表示へ変換されることを検証します。
    /// </summary>
    public void CreatePresentation_WhenUnexpectedException_ReturnsGenericMessage()
    {
        var wEx = new InvalidOperationException("unexpected");

        var wResult = ErrorHandlingPolicy.CreatePresentation(wEx);

        Assert.That(wResult.Category, Is.EqualTo(ErrorCategoryEnum.Unexpected));
        Assert.That(wResult.Title, Is.EqualTo("システムエラー"));
        Assert.That(wResult.UserMessage, Does.Contain("予期しないエラー"));
        Assert.That(wResult.LogLevel, Is.EqualTo("ERROR"));
    }
}

