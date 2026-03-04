using System;

namespace SalesManagementApp.Core.Application.Errors;

/// <summary>
/// ApplicationOperationException クラスです。
/// </summary>
public class ApplicationOperationException : Exception
{
    /// <summary>
    /// 公開メソッドです。
    /// </summary>
    public ApplicationOperationException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// 公開メソッドです。
    /// </summary>
    public ApplicationOperationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
