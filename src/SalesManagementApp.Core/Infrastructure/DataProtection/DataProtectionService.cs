using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SalesManagementApp.Core.Application.Exceptions;

namespace SalesManagementApp.Core.Infrastructure.DataProtection;

public class DataProtectionService
{
    private const string BackupDirectoryName = "_backup";
    private const string LogDirectoryName = "logs";
    private const string LogFileName = "operations.log";

    public string? CreateBackupIfExists(string filePath)
    {
        if (!File.Exists(filePath))
        {
            return null;
        }

        var backupDirectory = ResolveBackupDirectory(filePath);
        Directory.CreateDirectory(backupDirectory);

        var backupFileName = string.Format(
            "{0}_{1}{2}.bak",
            Path.GetFileNameWithoutExtension(filePath),
            DateTime.Now.ToString("yyyyMMdd_HHmmssfff"),
            Path.GetExtension(filePath));
        var backupPath = Path.Combine(backupDirectory, backupFileName);

        File.Copy(filePath, backupPath, overwrite: true);
        return backupPath;
    }

    public IReadOnlyList<string> GetBackupFiles(string filePath)
    {
        var backupDirectory = ResolveBackupDirectory(filePath);
        if (!Directory.Exists(backupDirectory))
        {
            return Array.Empty<string>();
        }

        return Directory
            .GetFiles(backupDirectory, "*.bak")
            .OrderByDescending(File.GetLastWriteTimeUtc)
            .ToList();
    }

    public void RestoreLatestBackup(string filePath)
    {
        var backupFile = GetBackupFiles(filePath).FirstOrDefault();
        if (string.IsNullOrWhiteSpace(backupFile))
        {
            throw new DomainValidationException("復元可能なバックアップが存在しません。");
        }

        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.Copy(backupFile, filePath, overwrite: true);
    }

    public void WriteLog(string filePath, string level, string message)
    {
        var targetDirectory = Path.GetDirectoryName(filePath);
        if (string.IsNullOrWhiteSpace(targetDirectory))
        {
            targetDirectory = Directory.GetCurrentDirectory();
        }

        var logDirectory = Path.Combine(targetDirectory, LogDirectoryName);
        Directory.CreateDirectory(logDirectory);

        var logPath = Path.Combine(logDirectory, LogFileName);
        var line = string.Format("{0:yyyy-MM-dd HH:mm:ss.fff} [{1}] {2}", DateTime.Now, level, message);
        File.AppendAllLines(logPath, new[] { line }, Encoding.UTF8);
    }

    private static string ResolveBackupDirectory(string filePath)
    {
        var baseDirectory = Path.GetDirectoryName(filePath);
        if (string.IsNullOrWhiteSpace(baseDirectory))
        {
            baseDirectory = Directory.GetCurrentDirectory();
        }

        return Path.Combine(baseDirectory, BackupDirectoryName);
    }
}
