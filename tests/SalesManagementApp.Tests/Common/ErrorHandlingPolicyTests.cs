using System;
using NUnit.Framework;
using SalesManagementApp.Core.Application.Errors;
using SalesManagementApp.Core.Application.Exceptions;

namespace SalesManagementApp.Tests.Common;

/// <summary>
/// ErrorHandlingPolicyTests クラスです。
/// </summary>
public class ErrorHandlingPolicyTests
{
    [Test]
    /// <summary>
    /// 公開メソッドです。
    /// </summary>
    public void CreatePresentation_WhenDomainValidationException_ReturnsValidationCategory()
    {
        var ex = new DomainValidationException("入力不備です。");

        var result = ErrorHandlingPolicy.CreatePresentation(ex);

        Assert.That(result.Category, Is.EqualTo(ErrorCategory.Validation));
        Assert.That(result.Title, Is.EqualTo("入力エラー"));
        Assert.That(result.UserMessage, Is.EqualTo("入力不備です。"));
        Assert.That(result.LogLevel, Is.EqualTo("WARN"));
    }

    [Test]
    /// <summary>
    /// 公開メソッドです。
    /// </summary>
    public void CreatePresentation_WhenApplicationOperationException_ReturnsOperationCategory()
    {
        var ex = new ApplicationOperationException("業務処理に失敗しました。");

        var result = ErrorHandlingPolicy.CreatePresentation(ex);

        Assert.That(result.Category, Is.EqualTo(ErrorCategory.Operation));
        Assert.That(result.Title, Is.EqualTo("業務エラー"));
        Assert.That(result.LogLevel, Is.EqualTo("ERROR"));
    }

    [Test]
    /// <summary>
    /// 公開メソッドです。
    /// </summary>
    public void CreatePresentation_WhenUnexpectedException_ReturnsGenericMessage()
    {
        var ex = new InvalidOperationException("unexpected");

        var result = ErrorHandlingPolicy.CreatePresentation(ex);

        Assert.That(result.Category, Is.EqualTo(ErrorCategory.Unexpected));
        Assert.That(result.Title, Is.EqualTo("システムエラー"));
        Assert.That(result.UserMessage, Does.Contain("予期しないエラー"));
        Assert.That(result.LogLevel, Is.EqualTo("ERROR"));
    }
}
