using System;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using SalesManagementApp.Core.Application.Exceptions;

namespace Sales_Management_App {
    public partial class Form1 {
        private static readonly Regex SalesFileNamePattern =
            new Regex(@"^sales_(\d{8})\.csv$", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

        private void LoadInitialDataFromRepositoryRoot() {
            var rootPath = FindRepositoryRoot(AppDomain.CurrentDomain.BaseDirectory);
            if (string.IsNullOrWhiteSpace(rootPath)) {
                return;
            }

            try {
                var productsPath = Path.Combine(rootPath, "products.csv");
                var inventoryPath = Path.Combine(rootPath, "inventory.csv");
                var salesPath = ResolveSalesPath(rootPath);
                var inventoryHistoryPath = Path.Combine(rootPath, "inventory_history.csv");
                _inventoryHistoryPath = inventoryHistoryPath;

                if (File.Exists(productsPath)) {
                    _products.Clear();
                    _products.AddRange(_csvDataStore.ReadProducts(productsPath));
                }

                if (File.Exists(inventoryPath)) {
                    _inventories.Clear();
                    _inventories.AddRange(_csvDataStore.ReadInventories(inventoryPath));
                }

                if (!string.IsNullOrWhiteSpace(salesPath) && File.Exists(salesPath)) {
                    _sales.Clear();
                    _sales.AddRange(_csvDataStore.ReadAndNormalizeSales(salesPath, _products));
                }

                if (File.Exists(inventoryHistoryPath)) {
                    _inventoryHistories.Clear();
                    _inventoryHistories.AddRange(_csvDataStore.ReadInventoryHistories(inventoryHistoryPath));
                }

                RefreshProductsGrid();
                RefreshInventoryGrid();
                RefreshInventoryHistoryGrid();
                RefreshSaleProductOptions();
                RefreshSalesGrid();
                ResetAggregationDisplay();
                UpdateSaleUnitPriceAndAmountPreview();
            } catch (DomainValidationException ex) {
                MessageBox.Show(string.Format("初期データの読み込みに失敗しました: {0}", ex.Message), "入力エラー", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            } catch (Exception ex) {
                MessageBox.Show(string.Format("初期データの読み込み中にエラーが発生しました: {0}", ex.Message), "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static string FindRepositoryRoot(string baseDirectory) {
            var current = new DirectoryInfo(baseDirectory);
            var depth = 0;

            while (current != null && depth < 10) {
                if (File.Exists(Path.Combine(current.FullName, "AGENTS.md"))) {
                    return current.FullName;
                }

                current = current.Parent;
                depth++;
            }

            return string.Empty;
        }

        private static string ResolveSalesPath(string rootPath) {
            var selectedPath = string.Empty;
            var selectedDate = DateTime.MinValue;
            var files = Directory.GetFiles(rootPath, "sales_*.csv");

            foreach (var filePath in files) {
                var fileName = Path.GetFileName(filePath);
                var match = SalesFileNamePattern.Match(fileName);
                if (!match.Success) {
                    continue;
                }

                DateTime parsedDate;
                if (!DateTime.TryParseExact(
                    match.Groups[1].Value,
                    "yyyyMMdd",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out parsedDate)) {
                    continue;
                }

                if (parsedDate <= selectedDate) {
                    continue;
                }

                selectedDate = parsedDate;
                selectedPath = filePath;
            }

            return selectedPath;
        }
    }
}
