using System.Collections.Generic;
using System.Windows.Forms;
using Sales_Management_App.Presentation.Common;

namespace Sales_Management_App {
    public partial class Form1 {
        private static readonly IReadOnlyDictionary<string, string> ProductGridHeaders =
            new Dictionary<string, string> {
                { "ProductId", "商品ID" },
                { "ProductName", "商品名" },
                { "UnitPrice", "単価" },
                { "Category", "区分" }
            };

        private static readonly IReadOnlyDictionary<string, string> InventoryGridHeaders =
            new Dictionary<string, string> {
                { "StoreId", "店舗ID" },
                { "ProductId", "商品ID" },
                { "Stock", "在庫数" }
            };

        private static readonly IReadOnlyDictionary<string, string> SalesGridHeaders =
            new Dictionary<string, string> {
                { "SaleDate", "販売日" },
                { "StoreId", "店舗ID" },
                { "ProductId", "商品ID" },
                { "ProductName", "商品名" },
                { "Quantity", "数量" },
                { "SalesAmount", "売上金額" }
            };

        private static readonly IReadOnlyDictionary<string, string> ProductSummaryGridHeaders =
            new Dictionary<string, string> {
                { "ProductId", "商品ID" },
                { "TotalQuantity", "販売数量" },
                { "TotalSalesAmount", "売上金額" }
            };

        private static readonly IReadOnlyDictionary<string, string> WeeklySummaryGridHeaders =
            new Dictionary<string, string> {
                { "Week", "週" },
                { "TotalQuantity", "販売数量" },
                { "TotalSalesAmount", "売上金額" }
            };

        private static readonly IReadOnlyDictionary<string, string> InventoryHistoryGridHeaders =
            new Dictionary<string, string> {
                { "OccurredAt", "日時" },
                { "OperationType", "操作種別" },
                { "StoreId", "店舗ID" },
                { "ProductId", "商品ID" },
                { "Quantity", "数量" },
                { "ResultStock", "実行後在庫" },
                { "Result", "実行結果" }
            };

        private static void ApplyJapaneseHeaders(DataGridView grid, IReadOnlyDictionary<string, string> headers) {
            DataGridHeaderMapper.Apply(grid, headers);
        }
    }
}
