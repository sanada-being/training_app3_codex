using System;

namespace SalesManagementApp.Core.Application.Errors;

public class ApplicationOperationException : Exception
{
    public ApplicationOperationException(string message)
        : base(message)
    {
    }

    public ApplicationOperationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
