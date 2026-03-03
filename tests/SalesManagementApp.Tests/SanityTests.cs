using NUnit.Framework;

namespace SalesManagementApp.Tests;

public class SanityTests
{
    [Test]
    public void TestInfrastructure_ShouldPass()
    {
        Assert.That(1 + 1, Is.EqualTo(2));
    }
}
