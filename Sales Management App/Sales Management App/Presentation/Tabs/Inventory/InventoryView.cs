using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Sales_Management_App.Presentation.Common;

namespace Sales_Management_App.Presentation.Tabs.Inventory {
    /// <summary>
    /// 在庫管理 タブのUI構成と表示更新を担当するビューです。
    /// </summary>
    internal sealed class InventoryView : UserControl {
        private static readonly IReadOnlyDictionary<string, string> FHeaderMap =
            new Dictionary<string, string> {
                { "StoreId", "店舗ID" },
                { "ProductId", "商品ID" },
                { "Stock", "在庫数" }
            };

        private readonly DataGridView FGrid;
        private readonly TextBox FStoreIdText;
        private readonly TextBox FProductIdText;
        private readonly TextBox FQuantityText;
        private readonly TextBox FFilterStoreIdText;
        private readonly TextBox FFilterProductIdText;
        private readonly Label FReorderLabel;

        internal event EventHandler AddRequested;
        internal event EventHandler RemoveRequested;
        internal event EventHandler ClearInputRequested;
        internal event EventHandler FilterRequested;
        internal event EventHandler FilterClearRequested;
        internal event EventHandler SelectedInventoryChanged;

        internal InventoryView() {
            Dock = DockStyle.Fill;

            var wRoot = new TableLayoutPanel {
                Dock = DockStyle.Fill,
                RowCount = 4,
                ColumnCount = 1
            };
            wRoot.RowStyles.Add(new RowStyle(SizeType.Absolute, 160));
            wRoot.RowStyles.Add(new RowStyle(SizeType.Absolute, 72));
            wRoot.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
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

            FStoreIdText = AddLabeledTextBox(wInputPanel, "店舗ID", 0, 0);
            FProductIdText = AddLabeledTextBox(wInputPanel, "商品ID", 2, 0);
            FQuantityText = AddLabeledTextBox(wInputPanel, "数量", 0, 1);

            var wButtonFlow = new FlowLayoutPanel {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = true
            };
            wButtonFlow.Controls.Add(CreateButton("入荷", delegate { AddRequested?.Invoke(this, EventArgs.Empty); }));
            wButtonFlow.Controls.Add(CreateButton("出庫", delegate { RemoveRequested?.Invoke(this, EventArgs.Empty); }));
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

            FFilterStoreIdText = AddLabeledTextBox(wFilterPanel, "絞込 店舗ID", 0, 0);
            FFilterProductIdText = AddLabeledTextBox(wFilterPanel, "絞込 商品ID", 2, 0);

            var wFilterButtons = new FlowLayoutPanel {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = true
            };
            wFilterButtons.Controls.Add(CreateButton("絞り込み", delegate { FilterRequested?.Invoke(this, EventArgs.Empty); }));
            wFilterButtons.Controls.Add(CreateButton("解除", delegate { FilterClearRequested?.Invoke(this, EventArgs.Empty); }));
            wFilterPanel.Controls.Add(wFilterButtons, 0, 1);
            wFilterPanel.SetColumnSpan(wFilterButtons, 4);

            FReorderLabel = new Label {
                Dock = DockStyle.Fill,
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft,
                Padding = new Padding(12, 0, 0, 0)
            };

            FGrid = new DataGridView {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AutoGenerateColumns = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false
            };
            FGrid.SelectionChanged += delegate { SelectedInventoryChanged?.Invoke(this, EventArgs.Empty); };

            wRoot.Controls.Add(wInputPanel, 0, 0);
            wRoot.Controls.Add(wFilterPanel, 0, 1);
            wRoot.Controls.Add(FReorderLabel, 0, 2);
            wRoot.Controls.Add(FGrid, 0, 3);
            Controls.Add(wRoot);
        }

        internal InventoryInputModel GetInput() {
            return new InventoryInputModel {
                StoreId = FStoreIdText.Text.Trim(),
                ProductId = FProductIdText.Text.Trim(),
                QuantityText = FQuantityText.Text.Trim()
            };
        }

        internal InventoryFilterModel GetFilter() {
            return new InventoryFilterModel {
                StoreId = FFilterStoreIdText.Text.Trim(),
                ProductId = FFilterProductIdText.Text.Trim()
            };
        }

        internal InventoryViewRow GetSelectedRow() {
            if (FGrid.SelectedRows.Count == 0) {
                return null;
            }

            var wRow = FGrid.SelectedRows[0];

            return new InventoryViewRow {
                StoreId = ToText(wRow.Cells["StoreId"].Value),
                ProductId = ToText(wRow.Cells["ProductId"].Value),
                Stock = ToInt32(wRow.Cells["Stock"].Value)
            };
        }

        internal void SetInput(InventoryInputModel vInput) {
            if (vInput == null) {
                FStoreIdText.Text = string.Empty;
                FProductIdText.Text = string.Empty;
                FQuantityText.Text = string.Empty;
                return;
            }

            FStoreIdText.Text = vInput.StoreId;
            FProductIdText.Text = vInput.ProductId;
            FQuantityText.Text = vInput.QuantityText;
        }

        internal void ClearInput() {
            FStoreIdText.Text = string.Empty;
            FProductIdText.Text = string.Empty;
            FQuantityText.Text = string.Empty;
        }

        internal void ClearFilter() {
            FFilterStoreIdText.Text = string.Empty;
            FFilterProductIdText.Text = string.Empty;
        }

        internal void SetRows(IReadOnlyCollection<InventoryViewRow> vRows) {
            FGrid.DataSource = null;
            FGrid.DataSource = vRows.ToList();
            DataGridHeaderMapper.Apply(FGrid, FHeaderMap);
        }

        internal void SetReorderCount(int vCount) {
            FReorderLabel.Text = string.Format("要発注（在庫5以下）件数: {0}", vCount);
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
