using System;

namespace SalesManagementApp.Core.Application.Exceptions;

/// <summary>
/// DomainValidationException クラスです。
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
