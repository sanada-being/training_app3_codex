using System;
using NUnit.Framework;
using SalesManagementApp.Core.Application.Exceptions;
using SalesManagementApp.Core.Application.Validation;

namespace SalesManagementApp.Tests.Common;

public class ValidationGuardTests
{
    [Test]
    public void RequireNotEmpty_WhenValueIsBlank_ThrowsDomainValidationException()
    {
        Assert.That(
            () => ValidationGuard.RequireNotEmpty(" ", "商品ID"),
            Throws.TypeOf<DomainValidationException>());
    }

    [Test]
    public void RequirePositive_WhenValueIsZero_ThrowsDomainValidationException()
    {
        Assert.That(
            () => ValidationGuard.RequirePositive(0, "数量"),
            Throws.TypeOf<DomainValidationException>());
    }

    [Test]
    public void RequireDateRange_WhenStartDateAfterEndDate_ThrowsDomainValidationException()
    {
        Assert.That(
            () => ValidationGuard.RequireDateRange(new DateTime(2026, 3, 10), new DateTime(2026, 3, 1)),
            Throws.TypeOf<DomainValidationException>());
    }
}
