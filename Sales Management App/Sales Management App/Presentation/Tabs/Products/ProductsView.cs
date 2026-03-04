using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Sales_Management_App.Presentation.Common;

namespace Sales_Management_App.Presentation.Tabs.Products {
    internal sealed class ProductsView : UserControl {
        private static readonly IReadOnlyDictionary<string, string> HeaderMap =
            new Dictionary<string, string> {
                { "ProductId", "商品ID" },
                { "ProductName", "商品名" },
                { "UnitPrice", "単価" },
                { "Category", "区分" }
            };

        private readonly DataGridView _grid;
        private readonly TextBox _productIdText;
        private readonly TextBox _productNameText;
        private readonly TextBox _unitPriceText;
        private readonly TextBox _categoryText;
        private readonly TextBox _filterProductIdText;
        private readonly TextBox _filterProductNameText;
        private readonly TextBox _filterCategoryText;

        internal event EventHandler RegisterRequested;
        internal event EventHandler UpdateRequested;
        internal event EventHandler DeleteRequested;
        internal event EventHandler ClearInputRequested;
        internal event EventHandler FilterRequested;
        internal event EventHandler FilterClearRequested;
        internal event EventHandler SelectedProductChanged;

        internal ProductsView() {
            Dock = DockStyle.Fill;

            var root = new TableLayoutPanel {
                Dock = DockStyle.Fill,
                RowCount = 3,
                ColumnCount = 1
            };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 170));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 96));
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

            _productIdText = AddLabeledTextBox(inputPanel, "商品ID", 0, 0);
            _productNameText = AddLabeledTextBox(inputPanel, "商品名", 2, 0);
            _unitPriceText = AddLabeledTextBox(inputPanel, "単価", 0, 1);
            _categoryText = AddLabeledTextBox(inputPanel, "区分", 2, 1);

            var buttonFlow = new FlowLayoutPanel {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = true
            };
            buttonFlow.Controls.Add(CreateButton("登録", delegate { RegisterRequested?.Invoke(this, EventArgs.Empty); }));
            buttonFlow.Controls.Add(CreateButton("更新", delegate { UpdateRequested?.Invoke(this, EventArgs.Empty); }));
            buttonFlow.Controls.Add(CreateButton("削除", delegate { DeleteRequested?.Invoke(this, EventArgs.Empty); }));
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

            _filterProductIdText = AddLabeledTextBox(filterPanel, "絞込 商品ID", 0, 0);
            _filterProductNameText = AddLabeledTextBox(filterPanel, "絞込 商品名", 2, 0);
            _filterCategoryText = AddLabeledTextBox(filterPanel, "絞込 区分", 0, 1);

            var filterButtonFlow = new FlowLayoutPanel {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = true
            };
            filterButtonFlow.Controls.Add(CreateButton("絞り込み", delegate { FilterRequested?.Invoke(this, EventArgs.Empty); }));
            filterButtonFlow.Controls.Add(CreateButton("解除", delegate { FilterClearRequested?.Invoke(this, EventArgs.Empty); }));
            filterPanel.Controls.Add(filterButtonFlow, 2, 1);
            filterPanel.SetColumnSpan(filterButtonFlow, 2);

            _grid = new DataGridView {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AutoGenerateColumns = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false
            };
            _grid.SelectionChanged += delegate { SelectedProductChanged?.Invoke(this, EventArgs.Empty); };

            root.Controls.Add(inputPanel, 0, 0);
            root.Controls.Add(filterPanel, 0, 1);
            root.Controls.Add(_grid, 0, 2);
            Controls.Add(root);
        }

        internal ProductInputModel GetInput() {
            return new ProductInputModel {
                ProductId = _productIdText.Text.Trim(),
                ProductName = _productNameText.Text.Trim(),
                UnitPriceText = _unitPriceText.Text.Trim(),
                Category = _categoryText.Text.Trim()
            };
        }

        internal ProductFilterModel GetFilter() {
            return new ProductFilterModel {
                ProductId = _filterProductIdText.Text.Trim(),
                ProductName = _filterProductNameText.Text.Trim(),
                Category = _filterCategoryText.Text.Trim()
            };
        }

        internal string GetDeleteTargetProductId() {
            return _productIdText.Text.Trim();
        }

        internal ProductViewRow GetSelectedRow() {
            if (_grid.SelectedRows.Count == 0) {
                return null;
            }

            var row = _grid.SelectedRows[0];

            return new ProductViewRow {
                ProductId = ToText(row.Cells["ProductId"].Value),
                ProductName = ToText(row.Cells["ProductName"].Value),
                UnitPrice = ToInt32(row.Cells["UnitPrice"].Value),
                Category = ToText(row.Cells["Category"].Value)
            };
        }

        internal void SetInput(ProductInputModel input) {
            if (input == null) {
                _productIdText.Text = string.Empty;
                _productNameText.Text = string.Empty;
                _unitPriceText.Text = string.Empty;
                _categoryText.Text = string.Empty;
                return;
            }

            _productIdText.Text = input.ProductId;
            _productNameText.Text = input.ProductName;
            _unitPriceText.Text = input.UnitPriceText;
            _categoryText.Text = input.Category;
        }

        internal void ClearInput() {
            _productIdText.Text = string.Empty;
            _productNameText.Text = string.Empty;
            _unitPriceText.Text = string.Empty;
            _categoryText.Text = string.Empty;
        }

        internal void ClearFilter() {
            _filterProductIdText.Text = string.Empty;
            _filterProductNameText.Text = string.Empty;
            _filterCategoryText.Text = string.Empty;
        }

        internal void SetRows(IReadOnlyCollection<ProductViewRow> rows) {
            _grid.DataSource = null;
            _grid.DataSource = rows.ToList();
            DataGridHeaderMapper.Apply(_grid, HeaderMap);
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
