using System;

namespace SalesManagementApp.Core.Application.Exceptions;

/// <summary>
/// ドメイン検証エラーを表す例外です。
/// </summary>
public class DomainValidationException : Exception
{
    /// <summary>
    /// 公開メソッドです。
    /// </summary>
    public DomainValidationException(string message) : base(message)
    {
    }
}
