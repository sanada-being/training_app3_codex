using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Sales_Management_App.Presentation.Common;

namespace Sales_Management_App.Presentation.Tabs.Products {
    /// <summary>
    /// 商品管理 タブのUI構成と表示更新を担当するビューです。
    /// </summary>
    internal sealed class ProductsView : UserControl {
        private static readonly IReadOnlyDictionary<string, string> FHeaderMap =
            new Dictionary<string, string> {
                { "ProductId", "商品ID" },
                { "ProductName", "商品名" },
                { "UnitPrice", "単価" },
                { "Category", "区分" }
            };

        private readonly DataGridView FGrid;
        private readonly TextBox FProductIdText;
        private readonly TextBox FProductNameText;
        private readonly TextBox FUnitPriceText;
        private readonly TextBox FCategoryText;
        private readonly TextBox FFilterProductIdText;
        private readonly TextBox FFilterProductNameText;
        private readonly TextBox FFilterCategoryText;

        internal event EventHandler RegisterRequested;
        internal event EventHandler UpdateRequested;
        internal event EventHandler DeleteRequested;
        internal event EventHandler ClearInputRequested;
        internal event EventHandler FilterRequested;
        internal event EventHandler FilterClearRequested;
        internal event EventHandler SelectedProductChanged;

        internal ProductsView() {
            Dock = DockStyle.Fill;

            var wRoot = new TableLayoutPanel {
                Dock = DockStyle.Fill,
                RowCount = 3,
                ColumnCount = 1
            };
            wRoot.RowStyles.Add(new RowStyle(SizeType.Absolute, 170));
            wRoot.RowStyles.Add(new RowStyle(SizeType.Absolute, 96));
            wRoot.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            var wInputPanel = new TableLayoutPanel {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 3,
                Padding = new Padding(12)
            };
            wInputPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            wInputPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            wInputPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            wInputPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

            FProductIdText = AddLabeledTextBox(wInputPanel, "商品ID", 0, 0);
            FProductNameText = AddLabeledTextBox(wInputPanel, "商品名", 2, 0);
            FUnitPriceText = AddLabeledTextBox(wInputPanel, "単価", 0, 1);
            FCategoryText = AddLabeledTextBox(wInputPanel, "区分", 2, 1);

            var wButtonFlow = new FlowLayoutPanel {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = true
            };
            wButtonFlow.Controls.Add(CreateButton("登録", delegate { RegisterRequested?.Invoke(this, EventArgs.Empty); }));
            wButtonFlow.Controls.Add(CreateButton("更新", delegate { UpdateRequested?.Invoke(this, EventArgs.Empty); }));
            wButtonFlow.Controls.Add(CreateButton("削除", delegate { DeleteRequested?.Invoke(this, EventArgs.Empty); }));
            wButtonFlow.Controls.Add(CreateButton("クリア", delegate { ClearInputRequested?.Invoke(this, EventArgs.Empty); }));
            wInputPanel.Controls.Add(wButtonFlow, 0, 2);
            wInputPanel.SetColumnSpan(wButtonFlow, 4);

            var wFilterPanel = new TableLayoutPanel {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 2,
                Padding = new Padding(12, 0, 12, 8)
            };
            wFilterPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            wFilterPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            wFilterPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            wFilterPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

            FFilterProductIdText = AddLabeledTextBox(wFilterPanel, "絞込 商品ID", 0, 0);
            FFilterProductNameText = AddLabeledTextBox(wFilterPanel, "絞込 商品名", 2, 0);
            FFilterCategoryText = AddLabeledTextBox(wFilterPanel, "絞込 区分", 0, 1);

            var wFilterButtonFlow = new FlowLayoutPanel {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = true
            };
            wFilterButtonFlow.Controls.Add(CreateButton("絞り込み", delegate { FilterRequested?.Invoke(this, EventArgs.Empty); }));
            wFilterButtonFlow.Controls.Add(CreateButton("解除", delegate { FilterClearRequested?.Invoke(this, EventArgs.Empty); }));
            wFilterPanel.Controls.Add(wFilterButtonFlow, 2, 1);
            wFilterPanel.SetColumnSpan(wFilterButtonFlow, 2);

            FGrid = new DataGridView {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AutoGenerateColumns = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false
            };
            FGrid.SelectionChanged += delegate { SelectedProductChanged?.Invoke(this, EventArgs.Empty); };

            wRoot.Controls.Add(wInputPanel, 0, 0);
            wRoot.Controls.Add(wFilterPanel, 0, 1);
            wRoot.Controls.Add(FGrid, 0, 2);
            Controls.Add(wRoot);
        }

        internal ProductInputModel GetInput() {
            return new ProductInputModel {
                ProductId = FProductIdText.Text.Trim(),
                ProductName = FProductNameText.Text.Trim(),
                UnitPriceText = FUnitPriceText.Text.Trim(),
                Category = FCategoryText.Text.Trim()
            };
        }

        internal ProductFilterModel GetFilter() {
            return new ProductFilterModel {
                ProductId = FFilterProductIdText.Text.Trim(),
                ProductName = FFilterProductNameText.Text.Trim(),
                Category = FFilterCategoryText.Text.Trim()
            };
        }

        internal string GetDeleteTargetProductId() {
            return FProductIdText.Text.Trim();
        }

        internal ProductViewRow GetSelectedRow() {
            if (FGrid.SelectedRows.Count == 0) {
                return null;
            }

            var wRow = FGrid.SelectedRows[0];

            return new ProductViewRow {
                ProductId = ToText(wRow.Cells["ProductId"].Value),
                ProductName = ToText(wRow.Cells["ProductName"].Value),
                UnitPrice = ToInt32(wRow.Cells["UnitPrice"].Value),
                Category = ToText(wRow.Cells["Category"].Value)
            };
        }

        internal void SetInput(ProductInputModel vInput) {
            if (vInput == null) {
                FProductIdText.Text = string.Empty;
                FProductNameText.Text = string.Empty;
                FUnitPriceText.Text = string.Empty;
                FCategoryText.Text = string.Empty;
                return;
            }

            FProductIdText.Text = vInput.ProductId;
            FProductNameText.Text = vInput.ProductName;
            FUnitPriceText.Text = vInput.UnitPriceText;
            FCategoryText.Text = vInput.Category;
        }

        internal void ClearInput() {
            FProductIdText.Text = string.Empty;
            FProductNameText.Text = string.Empty;
            FUnitPriceText.Text = string.Empty;
            FCategoryText.Text = string.Empty;
        }

        internal void ClearFilter() {
            FFilterProductIdText.Text = string.Empty;
            FFilterProductNameText.Text = string.Empty;
            FFilterCategoryText.Text = string.Empty;
        }

        internal void SetRows(IReadOnlyCollection<ProductViewRow> vRows) {
            FGrid.DataSource = null;
            FGrid.DataSource = vRows.ToList();
            DataGridHeaderMapper.Apply(FGrid, FHeaderMap);
        }

        private static string ToText(object vValue) {
            return vValue?.ToString() ?? string.Empty;
        }

        private static int ToInt32(object vValue) {
            int wParsed;
            return int.TryParse(ToText(vValue), out wParsed) ? wParsed : 0;
        }

        private static TextBox AddLabeledTextBox(TableLayoutPanel vPanel, string vLabel, int vCol, int vRow) {
            var wLbl = new Label {
                Text = vLabel,
                Dock = DockStyle.Fill,
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft
            };

            var wTextBox = new TextBox { Dock = DockStyle.Fill };
            vPanel.Controls.Add(wLbl, vCol, vRow);
            vPanel.Controls.Add(wTextBox, vCol + 1, vRow);
            return wTextBox;
        }

        private static Button CreateButton(string vText, EventHandler vOnClick) {
            var wButton = new Button {
                Text = vText,
                Width = 120,
                Height = 34,
                Margin = new Padding(0, 0, 12, 0)
            };
            wButton.Click += vOnClick;
            return wButton;
        }
    }
}
