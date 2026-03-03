using System;
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
            var root = new TableLayoutPanel {
                Dock = DockStyle.Fill,
                RowCount = 2,
                ColumnCount = 1
            };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 160));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            var filterPanel = new TableLayoutPanel {
                Dock = DockStyle.Fill,
                ColumnCount = 6,
                RowCount = 3,
                Padding = new Padding(12)
            };
            filterPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            filterPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            filterPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            filterPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            filterPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            filterPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 220));

            var startLabel = new Label {
                Text = "開始日時",
                Dock = DockStyle.Fill,
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft
            };
            _historyStartDatePicker = new DateTimePicker {
                Dock = DockStyle.Fill,
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "yyyy/MM/dd HH:mm:ss",
                ShowCheckBox = true,
                Checked = false
            };
            filterPanel.Controls.Add(startLabel, 0, 0);
            filterPanel.Controls.Add(_historyStartDatePicker, 1, 0);

            var endLabel = new Label {
                Text = "終了日時",
                Dock = DockStyle.Fill,
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft
            };
            _historyEndDatePicker = new DateTimePicker {
                Dock = DockStyle.Fill,
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "yyyy/MM/dd HH:mm:ss",
                ShowCheckBox = true,
                Checked = false
            };
            filterPanel.Controls.Add(endLabel, 2, 0);
            filterPanel.Controls.Add(_historyEndDatePicker, 3, 0);

            var operationLabel = new Label {
                Text = "操作種別",
                Dock = DockStyle.Fill,
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft
            };
            _historyOperationTypeCombo = new ComboBox {
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            filterPanel.Controls.Add(operationLabel, 4, 0);
            filterPanel.Controls.Add(_historyOperationTypeCombo, 5, 0);

            _historyStoreIdText = AddLabeledTextBox(filterPanel, "店舗ID", 0, 1);
            _historyProductIdText = AddLabeledTextBox(filterPanel, "商品ID", 2, 1);

            var buttonFlow = new FlowLayoutPanel {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = true
            };
            buttonFlow.Controls.Add(CreateButton("検索", SearchInventoryHistory));
            buttonFlow.Controls.Add(CreateButton("クリア", ClearInventoryHistoryFilter));
            filterPanel.Controls.Add(buttonFlow, 0, 2);
            filterPanel.SetColumnSpan(buttonFlow, 6);

            _inventoryHistoryGrid = new DataGridView {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AutoGenerateColumns = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false
            };

            root.Controls.Add(filterPanel, 0, 0);
            root.Controls.Add(_inventoryHistoryGrid, 0, 1);
            tab.Controls.Add(root);

            BindInventoryHistoryOperationOptions();
            return tab;
        }

        private TabPage CreateAggregationTab() {
            var tab = new TabPage("売上集計");
            var root = new TableLayoutPanel {
                Dock = DockStyle.Fill,
                RowCount = 3,
                ColumnCount = 1
            };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 154));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            var inputPanel = new TableLayoutPanel {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 4,
                Padding = new Padding(12)
            };
            inputPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            inputPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            inputPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            inputPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

            var startLabel = new Label {
                Text = "開始日",
                Dock = DockStyle.Fill,
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft
            };
            _summaryStartDatePicker = new DateTimePicker {
                Dock = DockStyle.Fill,
                Format = DateTimePickerFormat.Short,
                Value = DateTime.Today.AddDays(-6)
            };
            inputPanel.Controls.Add(startLabel, 0, 0);
            inputPanel.Controls.Add(_summaryStartDatePicker, 1, 0);

            var endLabel = new Label {
                Text = "終了日",
                Dock = DockStyle.Fill,
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft
            };
            _summaryEndDatePicker = new DateTimePicker {
                Dock = DockStyle.Fill,
                Format = DateTimePickerFormat.Short,
                Value = DateTime.Today
            };
            inputPanel.Controls.Add(endLabel, 2, 0);
            inputPanel.Controls.Add(_summaryEndDatePicker, 3, 0);

            var buttonFlow = new FlowLayoutPanel {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = true
            };
            buttonFlow.Controls.Add(CreateButton("集計実行", ExecuteAggregation));
            buttonFlow.Controls.Add(CreateButton("集計結果コピー", CopyAggregationResult));
            inputPanel.Controls.Add(buttonFlow, 0, 1);
            inputPanel.SetColumnSpan(buttonFlow, 4);

            _aggregationFilterProductIdText = AddLabeledTextBox(inputPanel, "絞込 商品ID", 0, 2);
            var aggregationFilterButtons = new FlowLayoutPanel {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = true
            };
            aggregationFilterButtons.Controls.Add(CreateButton("絞り込み", SearchAggregationFilter));
            aggregationFilterButtons.Controls.Add(CreateButton("解除", ClearAggregationFilter));
            inputPanel.Controls.Add(aggregationFilterButtons, 2, 2);
            inputPanel.SetColumnSpan(aggregationFilterButtons, 2);

            _summaryTotalLabel = new Label {
                Dock = DockStyle.Fill,
                Padding = new Padding(12, 0, 0, 0),
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft
            };

            var split = new SplitContainer {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Horizontal,
                SplitterDistance = 200
            };

            var productPanel = new TableLayoutPanel {
                Dock = DockStyle.Fill,
                RowCount = 2,
                ColumnCount = 1
            };
            productPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
            productPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            productPanel.Controls.Add(new Label {
                Text = "商品別集計",
                Dock = DockStyle.Fill,
                Padding = new Padding(8, 0, 0, 0),
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft
            }, 0, 0);

            _productSummaryGrid = new DataGridView {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AutoGenerateColumns = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false
            };
            productPanel.Controls.Add(_productSummaryGrid, 0, 1);

            var weeklyPanel = new TableLayoutPanel {
                Dock = DockStyle.Fill,
                RowCount = 2,
                ColumnCount = 1
            };
            weeklyPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
            weeklyPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            weeklyPanel.Controls.Add(new Label {
                Text = "週次集計",
                Dock = DockStyle.Fill,
                Padding = new Padding(8, 0, 0, 0),
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft
            }, 0, 0);

            _weeklySummaryGrid = new DataGridView {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AutoGenerateColumns = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false
            };
            weeklyPanel.Controls.Add(_weeklySummaryGrid, 0, 1);

            split.Panel1.Controls.Add(productPanel);
            split.Panel2.Controls.Add(weeklyPanel);

            root.Controls.Add(inputPanel, 0, 0);
            root.Controls.Add(_summaryTotalLabel, 0, 1);
            root.Controls.Add(split, 0, 2);
            tab.Controls.Add(root);
            return tab;
        }

        private static TextBox AddLabeledTextBox(TableLayoutPanel panel, string label, int col, int row) {
            var lbl = new Label {
                Text = label,
                Dock = DockStyle.Fill,
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft
            };

            var textBox = new TextBox { Dock = DockStyle.Fill };
            panel.Controls.Add(lbl, col, row);
            panel.Controls.Add(textBox, col + 1, row);
            return textBox;
        }

        private static Button CreateButton(string text, EventHandler onClick) {
            var button = new Button {
                Text = text,
                Width = 120,
                Height = 34,
                Margin = new Padding(0, 0, 12, 0)
            };
            button.Click += onClick;
            return button;
        }
    }
}
