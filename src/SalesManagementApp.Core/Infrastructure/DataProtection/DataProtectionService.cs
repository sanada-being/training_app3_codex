using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SalesManagementApp.Core.Application.Exceptions;

namespace SalesManagementApp.Core.Infrastructure.DataProtection;

/// <summary>
/// バックアップ復元と操作ログ出力を提供します。
/// </summary>
public class DataProtectionService
{
    private const string C_BackupDirectoryName = "_backup";
    private const string C_LogDirectoryName = "logs";
    private const string C_LogFileName = "operations.log";

    /// <summary>
    /// 既存ファイルがある場合にバックアップを作成します。
    /// </summary>
    public string? CreateBackupIfExists(string vFilePath)
    {
        if (!File.Exists(vFilePath))
        {
            return null;
        }

        var wBackupDirectory = ResolveBackupDirectory(vFilePath);
        Directory.CreateDirectory(wBackupDirectory);

        var wBackupFileName = string.Format(
            "{0}_{1}{2}.bak",
            Path.GetFileNameWithoutExtension(vFilePath),
            DateTime.Now.ToString("yyyyMMdd_HHmmssfff"),
            Path.GetExtension(vFilePath));
        var wBackupPath = Path.Combine(wBackupDirectory, wBackupFileName);

        File.Copy(vFilePath, wBackupPath, overwrite: true);
        return wBackupPath;
    }

    /// <summary>
    /// 対象ファイルのバックアップ一覧を取得します。
    /// </summary>
    public IReadOnlyList<string> GetBackupFiles(string vFilePath)
    {
        var wBackupDirectory = ResolveBackupDirectory(vFilePath);
        if (!Directory.Exists(wBackupDirectory))
        {
            return Array.Empty<string>();
        }

        return Directory
            .GetFiles(wBackupDirectory, "*.bak")
            .OrderByDescending(File.GetLastWriteTimeUtc)
            .ToList();
    }

    /// <summary>
    /// 対象ファイルを最新バックアップで復元します。
    /// </summary>
    public void RestoreLatestBackup(string vFilePath)
    {
        var wBackupFile = GetBackupFiles(vFilePath).FirstOrDefault();
        if (string.IsNullOrWhiteSpace(wBackupFile))
        {
            throw new DomainValidationException("復元可能なバックアップが存在しません。");
        }

        var wDirectory = Path.GetDirectoryName(vFilePath);
        if (!string.IsNullOrWhiteSpace(wDirectory))
        {
            Directory.CreateDirectory(wDirectory);
        }

        File.Copy(wBackupFile, vFilePath, overwrite: true);
    }

    /// <summary>
    /// 操作ログをログファイルに追記します。
    /// </summary>
    public void WriteLog(string vFilePath, string vLevel, string vMessage)
    {
        var wTargetDirectory = Path.GetDirectoryName(vFilePath);
        if (string.IsNullOrWhiteSpace(wTargetDirectory))
        {
            wTargetDirectory = Directory.GetCurrentDirectory();
        }

        var wLogDirectory = Path.Combine(wTargetDirectory, C_LogDirectoryName);
        Directory.CreateDirectory(wLogDirectory);

        var wLogPath = Path.Combine(wLogDirectory, C_LogFileName);
        var wLine = string.Format("{0:yyyy-MM-dd HH:mm:ss.fff} [{1}] {2}", DateTime.Now, vLevel, vMessage);
        File.AppendAllLines(wLogPath, new[] { wLine }, Encoding.UTF8);
    }

    private static string ResolveBackupDirectory(string vFilePath)
    {
        var wBaseDirectory = Path.GetDirectoryName(vFilePath);
        if (string.IsNullOrWhiteSpace(wBaseDirectory))
        {
            wBaseDirectory = Directory.GetCurrentDirectory();
        }

        return Path.Combine(wBaseDirectory, C_BackupDirectoryName);
    }
}
