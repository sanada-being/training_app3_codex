using System;

namespace SalesManagementApp.Core.Application.Exceptions;

/// <summary>
/// ドメイン検証エラーを表す例外です。
/// </summary>
public class DomainValidationException : Exception
{
    /// <summary>
    /// ドメイン検証エラー例外を初期化します。
    /// </summary>
    public DomainValidationException(string vMessage) : base(vMessage)
    {
    }
}
