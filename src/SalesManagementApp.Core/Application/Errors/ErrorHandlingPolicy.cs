using System;
using SalesManagementApp.Core.Application.Exceptions;

namespace SalesManagementApp.Core.Application.Errors;

public enum ErrorCategory
{
    Validation,
    Operation,
    Unexpected
}

public class ErrorPresentation
{
    public string Title { get; set; } = string.Empty;
    public string UserMessage { get; set; } = string.Empty;
    public string LogLevel { get; set; } = string.Empty;
    public ErrorCategory Category { get; set; }
}

public static class ErrorHandlingPolicy
{
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
