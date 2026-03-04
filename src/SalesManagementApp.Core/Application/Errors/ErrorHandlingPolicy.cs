using System;
using SalesManagementApp.Core.Application.Exceptions;

namespace SalesManagementApp.Core.Application.Errors;

/// <summary>
/// 列挙体です。
/// </summary>
public enum ErrorCategory
{
    Validation,
    Operation,
    Unexpected
}

/// <summary>
/// ErrorPresentation クラスです。
/// </summary>
public class ErrorPresentation
{
    /// <summary>
    /// 公開プロパティです。
    /// </summary>
    public string Title { get; set; } = string.Empty;
    /// <summary>
    /// 公開プロパティです。
    /// </summary>
    public string UserMessage { get; set; } = string.Empty;
    /// <summary>
    /// 公開プロパティです。
    /// </summary>
    public string LogLevel { get; set; } = string.Empty;
    /// <summary>
    /// 公開プロパティです。
    /// </summary>
    public ErrorCategory Category { get; set; }
}

/// <summary>
/// ErrorHandlingPolicy クラスです。
/// </summary>
public static class ErrorHandlingPolicy
{
    /// <summary>
    /// 公開メソッドです。
    /// </summary>
    public static ErrorPresentation CreatePresentation(Exception ex)
    {
        if (ex is DomainValidationException)
        {
            return new ErrorPresentation
            {
                Title = "入力エラー",
                UserMessage = ex.Message,
                Category = ErrorCategory.Validation,
                LogLevel = "WARN"
            };
        }

        if (ex is ApplicationOperationException)
        {
            return new ErrorPresentation
            {
                Title = "業務エラー",
                UserMessage = ex.Message,
                Category = ErrorCategory.Operation,
                LogLevel = "ERROR"
            };
        }

        return new ErrorPresentation
        {
            Title = "システムエラー",
            UserMessage = "予期しないエラーが発生しました。管理者に連絡してください。",
            Category = ErrorCategory.Unexpected,
            LogLevel = "ERROR"
        };
    }
}
