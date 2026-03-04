using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Sales_Management_App.Presentation.Common;

namespace Sales_Management_App.Presentation.Tabs.Inventory {
    internal sealed class InventoryView : UserControl {
        private static readonly IReadOnlyDictionary<string, string> HeaderMap =
            new Dictionary<string, string> {
                { "StoreId", "店舗ID" },
                { "ProductId", "商品ID" },
                { "Stock", "在庫数" }
            };

        private readonly DataGridView _grid;
        private readonly TextBox _storeIdText;
        private readonly TextBox _productIdText;
        private readonly TextBox _quantityText;
        private readonly TextBox _filterStoreIdText;
        private readonly TextBox _filterProductIdText;
        private readonly Label _reorderLabel;

        internal event EventHandler AddRequested;
        internal event EventHandler RemoveRequested;
        internal event EventHandler ClearInputRequested;
        internal event EventHandler FilterRequested;
        internal event EventHandler FilterClearRequested;
        internal event EventHandler SelectedInventoryChanged;

        internal InventoryView() {
            Dock = DockStyle.Fill;

            var root = new TableLayoutPanel {
                Dock = DockStyle.Fill,
                RowCount = 4,
                ColumnCount = 1
            };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 160));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 72));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            var inputPanel = new TableLayoutPanel {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 3,
                Padding = new Padding(12)
            };
            inputPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            inputPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            inputPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            inputPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

            _storeIdText = AddLabeledTextBox(inputPanel, "店舗ID", 0, 0);
            _productIdText = AddLabeledTextBox(inputPanel, "商品ID", 2, 0);
            _quantityText = AddLabeledTextBox(inputPanel, "数量", 0, 1);

            var buttonFlow = new FlowLayoutPanel {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = true
            };
            buttonFlow.Controls.Add(CreateButton("入荷", delegate { AddRequested?.Invoke(this, EventArgs.Empty); }));
            buttonFlow.Controls.Add(CreateButton("出庫", delegate { RemoveRequested?.Invoke(this, EventArgs.Empty); }));
            buttonFlow.Controls.Add(CreateButton("クリア", delegate { ClearInputRequested?.Invoke(this, EventArgs.Empty); }));
            inputPanel.Controls.Add(buttonFlow, 0, 2);
            inputPanel.SetColumnSpan(buttonFlow, 4);

            var filterPanel = new TableLayoutPanel {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 2,
                Padding = new Padding(12, 0, 12, 8)
            };
            filterPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            filterPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            filterPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            filterPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

            _filterStoreIdText = AddLabeledTextBox(filterPanel, "絞込 店舗ID", 0, 0);
            _filterProductIdText = AddLabeledTextBox(filterPanel, "絞込 商品ID", 2, 0);

            var filterButtons = new FlowLayoutPanel {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = true
            };
            filterButtons.Controls.Add(CreateButton("絞り込み", delegate { FilterRequested?.Invoke(this, EventArgs.Empty); }));
            filterButtons.Controls.Add(CreateButton("解除", delegate { FilterClearRequested?.Invoke(this, EventArgs.Empty); }));
            filterPanel.Controls.Add(filterButtons, 0, 1);
            filterPanel.SetColumnSpan(filterButtons, 4);

            _reorderLabel = new Label {
                Dock = DockStyle.Fill,
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft,
                Padding = new Padding(12, 0, 0, 0)
            };

            _grid = new DataGridView {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AutoGenerateColumns = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false
            };
            _grid.SelectionChanged += delegate { SelectedInventoryChanged?.Invoke(this, EventArgs.Empty); };

            root.Controls.Add(inputPanel, 0, 0);
            root.Controls.Add(filterPanel, 0, 1);
            root.Controls.Add(_reorderLabel, 0, 2);
            root.Controls.Add(_grid, 0, 3);
            Controls.Add(root);
        }

        internal InventoryInputModel GetInput() {
            return new InventoryInputModel {
                StoreId = _storeIdText.Text.Trim(),
                ProductId = _productIdText.Text.Trim(),
                QuantityText = _quantityText.Text.Trim()
            };
        }

        internal InventoryFilterModel GetFilter() {
            return new InventoryFilterModel {
                StoreId = _filterStoreIdText.Text.Trim(),
                ProductId = _filterProductIdText.Text.Trim()
            };
        }

        internal InventoryViewRow GetSelectedRow() {
            if (_grid.SelectedRows.Count == 0) {
                return null;
            }

            var row = _grid.SelectedRows[0];

            return new InventoryViewRow {
                StoreId = ToText(row.Cells["StoreId"].Value),
                ProductId = ToText(row.Cells["ProductId"].Value),
                Stock = ToInt32(row.Cells["Stock"].Value)
            };
        }

        internal void SetInput(InventoryInputModel input) {
            if (input == null) {
                _storeIdText.Text = string.Empty;
                _productIdText.Text = string.Empty;
                _quantityText.Text = string.Empty;
                return;
            }

            _storeIdText.Text = input.StoreId;
            _productIdText.Text = input.ProductId;
            _quantityText.Text = input.QuantityText;
        }

        internal void ClearInput() {
            _storeIdText.Text = string.Empty;
            _productIdText.Text = string.Empty;
            _quantityText.Text = string.Empty;
        }

        internal void ClearFilter() {
            _filterStoreIdText.Text = string.Empty;
            _filterProductIdText.Text = string.Empty;
        }

        internal void SetRows(IReadOnlyCollection<InventoryViewRow> rows) {
            _grid.DataSource = null;
            _grid.DataSource = rows.ToList();
            DataGridHeaderMapper.Apply(_grid, HeaderMap);
        }

        internal void SetReorderCount(int count) {
            _reorderLabel.Text = string.Format("要発注（在庫5以下）件数: {0}", count);
        }

        private static string ToText(object value) {
            return value?.ToString() ?? string.Empty;
        }

        private static int ToInt32(object value) {
            int parsed;
            return int.TryParse(ToText(value), out parsed) ? parsed : 0;
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
