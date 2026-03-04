using System;

namespace SalesManagementApp.Core.Application.Errors;

/// <summary>
/// アプリケーション操作中の業務例外を表します。
/// </summary>
public class ApplicationOperationException : Exception
{
    /// <summary>
    /// 業務操作エラー例外を初期化します。
    /// </summary>
    public ApplicationOperationException(string vMessage)
        : base(vMessage)
    {
    }

    /// <summary>
    /// 業務操作エラー例外を初期化します。
    /// </summary>
    public ApplicationOperationException(string vMessage, Exception vInnerException)
        : base(vMessage, vInnerException)
    {
    }
}
