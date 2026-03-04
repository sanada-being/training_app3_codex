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
    /// 公開メソッドです。
    /// </summary>
    public AppBootstrapper()
        : this(new AppDataRepository())
    {
    }

    internal AppBootstrapper(AppDataRepository repository)
    {
        FRepository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    /// <summary>
    /// 公開メソッドです。
    /// </summary>
    public AppState LoadFromBaseDirectory(string baseDirectory)
    {
        var rootPath = FindRepositoryRoot(baseDirectory);
        if (string.IsNullOrWhiteSpace(rootPath))
        {
            return new AppState();
        }

        var state = new AppState
        {
            RepositoryRootPath = rootPath,
            ProductsPath = Path.Combine(rootPath, "products.csv"),
            InventoryPath = Path.Combine(rootPath, "inventory.csv"),
            InventoryHistoryPath = Path.Combine(rootPath, "inventory_history.csv")
        };
        state.SalesPath = ResolveSalesPath(rootPath);

        state.Products.AddRange(FRepository.ReadProductsIfExists(state.ProductsPath));
        state.Inventories.AddRange(FRepository.ReadInventoriesIfExists(state.InventoryPath));
        state.Sales.AddRange(FRepository.ReadAndNormalizeSalesIfExists(state.SalesPath, state.Products));
        state.InventoryHistories.AddRange(FRepository.ReadInventoryHistoriesIfExists(state.InventoryHistoryPath));

        return state;
    }

    private static string FindRepositoryRoot(string baseDirectory)
    {
        var current = new DirectoryInfo(baseDirectory);
        var depth = 0;

        while (current is not null && depth < 10)
        {
            if (File.Exists(Path.Combine(current.FullName, "AGENTS.md")))
            {
                return current.FullName;
            }

            current = current.Parent;
            depth++;
        }

        return string.Empty;
    }

    private static string ResolveSalesPath(string rootPath)
    {
        var selectedPath = string.Empty;
        var selectedDate = DateTime.MinValue;
        var files = Directory.GetFiles(rootPath, "sales_*.csv");

        foreach (var filePath in files)
        {
            var fileName = Path.GetFileName(filePath);
            var match = FSalesFileNamePattern.Match(fileName);
            if (!match.Success)
            {
                continue;
            }

            if (!DateTime.TryParseExact(
                    match.Groups[1].Value,
                    "yyyyMMdd",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var parsedDate))
            {
                continue;
            }

            if (parsedDate <= selectedDate)
            {
                continue;
            }

            selectedDate = parsedDate;
            selectedPath = filePath;
        }

        return selectedPath;
    }
}
