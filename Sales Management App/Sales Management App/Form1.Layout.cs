using System.Windows.Forms;

namespace Sales_Management_App {
    public partial class Form1 {
        private TabPage CreateProductTab() {
            var tab = new TabPage("商品管理");
            _productsView = new Presentation.Tabs.Products.ProductsView();
            tab.Controls.Add(_productsView);
            return tab;
        }

        private TabPage CreateInventoryTab() {
            var tab = new TabPage("在庫管理");
            _inventoryView = new Presentation.Tabs.Inventory.InventoryView();
            tab.Controls.Add(_inventoryView);
            return tab;
        }

        private TabPage CreateSalesTab() {
            var tab = new TabPage("売上登録");
            _salesView = new Presentation.Tabs.Sales.SalesView();
            tab.Controls.Add(_salesView);
            return tab;
        }

        private TabPage CreateInventoryHistoryTab() {
            var tab = new TabPage("在庫履歴");
            _inventoryHistoryView = new Presentation.Tabs.InventoryHistory.InventoryHistoryView();
            tab.Controls.Add(_inventoryHistoryView);
            return tab;
        }

        private TabPage CreateAggregationTab() {
            var tab = new TabPage("売上集計");
            _aggregationView = new Presentation.Tabs.Aggregation.AggregationView();
            tab.Controls.Add(_aggregationView);
            return tab;
        }
    }
}
