using System;
using System.IO;
using NUnit.Framework;
using SalesManagementApp.Core.Application.Exceptions;
using SalesManagementApp.Core.Infrastructure.DataProtection;

namespace SalesManagementApp.Tests.Csv;

/// <summary>
/// DataProtectionService の仕様を検証するNUnitテストクラスです。
/// </summary>
public class DataProtectionServiceTests
{
    private string FWorkDir = string.Empty;
    private DataProtectionService FService = null!;

    [SetUp]
    /// <summary>
    /// 各テストの実行前にテストデータと依存オブジェクトを初期化します。
    /// </summary>
    public void SetUp()
    {
        FWorkDir = Path.Combine(Path.GetTempPath(), "SalesManagementApp.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(FWorkDir);
        FService = new DataProtectionService();
    }

    [TearDown]
    /// <summary>
    /// 各テストの実行後に作業ディレクトリやリソースをクリーンアップします。
    /// </summary>
    public void TearDown()
    {
        if (Directory.Exists(FWorkDir))
        {
            Directory.Delete(FWorkDir, true);
        }
    }

    [Test]
    /// <summary>
    /// 対象ファイルが存在しない場合はバックアップを作成せず null を返すことを検証します。
    /// </summary>
    public void CreateBackupIfExists_WhenFileDoesNotExist_ReturnsNull()
    {
        var path = Path.Combine(FWorkDir, "missing.csv");

        var backup = FService.CreateBackupIfExists(path);

        Assert.That(backup, Is.Null);
    }

    [Test]
    /// <summary>
    /// 復元対象のバックアップが存在しない場合に検証例外が発生することを確認します。
    /// </summary>
    public void RestoreLatestBackup_WhenNoBackupExists_ThrowsValidationException()
    {
        var path = Path.Combine(FWorkDir, "products.csv");

        Assert.That(
            () => FService.RestoreLatestBackup(path),
            Throws.TypeOf<DomainValidationException>());
    }
}
