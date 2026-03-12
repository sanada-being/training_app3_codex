using System;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

namespace SalesManagementApp.Core.Application.State;

/// <summary>
/// 起動時にCSVからアプリケーション状態を復元します。
/// </summary>
public class AppBootstrapper
{
    private static readonly Regex FSalesFileNamePattern =
        new(@"^sales_(\d{8})\.csv$", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

    private readonly AppDataRepository FRepository;

    /// <summary>
    /// 起動時データ復元に必要な依存オブジェクトを初期化します。
    /// </summary>
    public AppBootstrapper()
        : this(new AppDataRepository())
    {
    }

    internal AppBootstrapper(AppDataRepository vRepository)
    {
        FRepository = vRepository ?? throw new ArgumentNullException(nameof(vRepository));
    }

    /// <summary>
    /// 基準ディレクトリからCSVを探索しアプリ状態を復元します。
    /// </summary>
    public AppState LoadFromBaseDirectory(string vBaseDirectory)
    {
        if (string.IsNullOrWhiteSpace(vBaseDirectory))
        {
            return new AppState();
        }

        var wRootPath = ResolveDataRoot(vBaseDirectory);
        var wState = new AppState
        {
            RepositoryRootPath = wRootPath,
            ProductsPath = Path.Combine(wRootPath, "products.csv"),
            InventoryPath = Path.Combine(wRootPath, "inventory.csv"),
            InventoryHistoryPath = Path.Combine(wRootPath, "inventory_history.csv")
        };
        wState.SalesPath = ResolveSalesPath(wRootPath);

        wState.Products.AddRange(FRepository.ReadProductsIfExists(wState.ProductsPath));
        wState.Inventories.AddRange(FRepository.ReadInventoriesIfExists(wState.InventoryPath));
        wState.Sales.AddRange(FRepository.ReadAndNormalizeSalesIfExists(wState.SalesPath, wState.Products));
        wState.InventoryHistories.AddRange(FRepository.ReadInventoryHistoriesIfExists(wState.InventoryHistoryPath));

        return wState;
    }

    private static string ResolveDataRoot(string vBaseDirectory)
    {
        var wRepositoryRoot = FindRepositoryRoot(vBaseDirectory);
        if (!string.IsNullOrWhiteSpace(wRepositoryRoot))
        {
            return wRepositoryRoot;
        }

        return Path.GetFullPath(vBaseDirectory);
    }

    private static string FindRepositoryRoot(string vBaseDirectory)
    {
        var wCurrent = new DirectoryInfo(vBaseDirectory);
        var wDepth = 0;

        while (wCurrent is not null && wDepth < 10)
        {
            if (File.Exists(Path.Combine(wCurrent.FullName, "AGENTS.md")))
            {
                return wCurrent.FullName;
            }

            wCurrent = wCurrent.Parent;
            wDepth++;
        }

        return string.Empty;
    }

    private static string ResolveSalesPath(string vRootPath)
    {
        var wSelectedPath = string.Empty;
        var wSelectedDate = DateTime.MinValue;
        var wFiles = Directory.GetFiles(vRootPath, "sales_*.csv");

        foreach (var wFilePath in wFiles)
        {
            var wFileName = Path.GetFileName(wFilePath);
            var wMatch = FSalesFileNamePattern.Match(wFileName);
            if (!wMatch.Success)
            {
                continue;
            }

            if (!DateTime.TryParseExact(
                    wMatch.Groups[1].Value,
                    "yyyyMMdd",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var wParsedDate))
            {
                continue;
            }

            if (wParsedDate <= wSelectedDate)
            {
                continue;
            }

            wSelectedDate = wParsedDate;
            wSelectedPath = wFilePath;
        }

        return wSelectedPath;
    }
}
