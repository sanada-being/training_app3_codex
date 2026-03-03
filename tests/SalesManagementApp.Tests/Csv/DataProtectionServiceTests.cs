using System;
using System.IO;
using NUnit.Framework;
using SalesManagementApp.Core.Application.Exceptions;
using SalesManagementApp.Core.Infrastructure.DataProtection;

namespace SalesManagementApp.Tests.Csv;

public class DataProtectionServiceTests
{
    private string _workDir = string.Empty;
    private DataProtectionService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _workDir = Path.Combine(Path.GetTempPath(), "SalesManagementApp.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_workDir);
        _service = new DataProtectionService();
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(_workDir))
        {
            Directory.Delete(_workDir, true);
        }
    }

    [Test]
    public void CreateBackupIfExists_WhenFileDoesNotExist_ReturnsNull()
    {
        var path = Path.Combine(_workDir, "missing.csv");

        var backup = _service.CreateBackupIfExists(path);

        Assert.That(backup, Is.Null);
    }

    [Test]
    public void RestoreLatestBackup_WhenNoBackupExists_ThrowsValidationException()
    {
        var path = Path.Combine(_workDir, "products.csv");

        Assert.That(
            () => _service.RestoreLatestBackup(path),
            Throws.TypeOf<DomainValidationException>());
    }
}
