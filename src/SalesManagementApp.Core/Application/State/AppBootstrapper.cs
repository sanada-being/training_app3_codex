using System;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

namespace SalesManagementApp.Core.Application.State;

public class AppBootstrapper
{
    private static readonly Regex SalesFileNamePattern =
        new(@"^sales_(\d{8})\.csv$", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

    private readonly AppDataRepository _repository;

    public AppBootstrapper()
        : this(new AppDataRepository())
    {
    }

    internal AppBootstrapper(AppDataRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

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

        state.Products.AddRange(_repository.ReadProductsIfExists(state.ProductsPath));
        state.Inventories.AddRange(_repository.ReadInventoriesIfExists(state.InventoryPath));
        state.Sales.AddRange(_repository.ReadAndNormalizeSalesIfExists(state.SalesPath, state.Products));
        state.InventoryHistories.AddRange(_repository.ReadInventoryHistoriesIfExists(state.InventoryHistoryPath));

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
            var match = SalesFileNamePattern.Match(fileName);
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
