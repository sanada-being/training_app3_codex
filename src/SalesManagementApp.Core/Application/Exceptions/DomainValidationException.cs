using System;

namespace SalesManagementApp.Core.Application.Exceptions;

public class DomainValidationException : Exception
{
    public DomainValidationException(string message) : base(message)
    {
    }
}
