using System;
using SalesManagementApp.Core.Application.Exceptions;

namespace SalesManagementApp.Core.Application.Errors;

/// <summary>
/// 例外を画面表示用途で分類する種別を定義します。
/// </summary>
public enum ErrorCategory
{
    /// <summary>
    /// 入力値や業務ルール違反によるエラーです。
    /// </summary>
    Validation,
    /// <summary>
    /// 業務処理の実行失敗を示すエラーです。
    /// </summary>
    Operation,
    /// <summary>
    /// 想定外の例外に分類されるエラーです。
    /// </summary>
    Unexpected
}

/// <summary>
/// 画面表示向けのエラー情報を保持します。
/// </summary>
public class ErrorPresentation
{
    /// <summary>
    /// ダイアログや通知に表示するエラータイトルです。
    /// </summary>
    public string Title { get; set; } = string.Empty;
    /// <summary>
    /// ユーザーに提示する説明メッセージです。
    /// </summary>
    public string UserMessage { get; set; } = string.Empty;
    /// <summary>
    /// ログ出力時に使用する優先度レベルです。
    /// </summary>
    public string LogLevel { get; set; } = string.Empty;
    /// <summary>
    /// エラーの取り扱い区分を示すカテゴリです。
    /// </summary>
    public ErrorCategory Category { get; set; }
}

/// <summary>
/// 例外をユーザー向けエラー情報へ変換する方針を提供します。
/// </summary>
public static class ErrorHandlingPolicy
{
    /// <summary>
    /// 例外種別に応じた画面表示用エラー情報を生成します。
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
