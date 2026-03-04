using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Sales_Management_App.Presentation.Common;

namespace Sales_Management_App.Presentation.Tabs.Sales {
    /// <summary>
    /// 売上登録 タブのUI構成と表示更新を担当するビューです。
    /// </summary>
    internal sealed class SalesView : UserControl {
        private static readonly IReadOnlyDictionary<string, string> FHeaderMap =
            new Dictionary<string, string> {
                { "SaleDate", "販売日" },
                { "StoreId", "店舗ID" },
                { "ProductId", "商品ID" },
                { "ProductName", "商品名" },
                { "Quantity", "数量" },
                { "SalesAmount", "売上金額" }
            };

        private readonly DataGridView FGrid;
        private readonly DateTimePicker FSaleDatePicker;
        private readonly TextBox FStoreIdText;
        private readonly ComboBox FProductCombo;
        private readonly TextBox FQuantityText;
        private readonly Label FUnitPriceLabel;
        private readonly Label FAmountPreviewLabel;
        private readonly DateTimePicker FFilterStartDatePicker;
        private readonly DateTimePicker FFilterEndDatePicker;
        private readonly TextBox FFilterStoreIdText;
        private readonly TextBox FFilterProductIdText;

        internal event EventHandler RegisterRequested;
        internal event EventHandler ClearInputRequested;
        internal event EventHandler FilterRequested;
        internal event EventHandler FilterClearRequested;
        internal event EventHandler InputChanged;

        internal SalesView() {
            Dock = DockStyle.Fill;

            var root = new TableLayoutPanel {
                Dock = DockStyle.Fill,
                RowCount = 4,
                ColumnCount = 1
            };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 200));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 120));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
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

            var saleDateLabel = new Label {
                Text = "販売日",
                Dock = DockStyle.Fill,
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft
            };
            FSaleDatePicker = new DateTimePicker {
                Dock = DockStyle.Fill,
                Format = DateTimePickerFormat.Short
            };
            inputPanel.Controls.Add(saleDateLabel, 0, 0);
            inputPanel.Controls.Add(FSaleDatePicker, 1, 0);

            FStoreIdText = AddLabeledTextBox(inputPanel, "店舗ID", 2, 0);

            var productLabel = new Label {
                Text = "商品",
                Dock = DockStyle.Fill,
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft
            };
            FProductCombo = new ComboBox {
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            inputPanel.Controls.Add(productLabel, 0, 1);
            inputPanel.Controls.Add(FProductCombo, 1, 1);

            FQuantityText = AddLabeledTextBox(inputPanel, "数量", 2, 1);

            var unitPriceHeaderLabel = new Label {
                Text = "単価",
                Dock = DockStyle.Fill,
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft
            };
            FUnitPriceLabel = new Label {
                Dock = DockStyle.Fill,
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft
            };
            inputPanel.Controls.Add(unitPriceHeaderLabel, 0, 2);
            inputPanel.Controls.Add(FUnitPriceLabel, 1, 2);

            var amountHeaderLabel = new Label {
                Text = "売上金額見込",
                Dock = DockStyle.Fill,
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft
            };
            FAmountPreviewLabel = new Label {
                Dock = DockStyle.Fill,
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft
            };
            inputPanel.Controls.Add(amountHeaderLabel, 2, 2);
            inputPanel.Controls.Add(FAmountPreviewLabel, 3, 2);

            var buttonFlow = new FlowLayoutPanel {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = true
            };
            buttonFlow.Controls.Add(CreateButton("売上登録", delegate { RegisterRequested?.Invoke(this, EventArgs.Empty); }));
            buttonFlow.Controls.Add(CreateButton("クリア", delegate { ClearInputRequested?.Invoke(this, EventArgs.Empty); }));
            inputPanel.Controls.Add(buttonFlow, 0, 3);
            inputPanel.SetColumnSpan(buttonFlow, 4);

            var filterPanel = new TableLayoutPanel {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 3,
                Padding = new Padding(12, 0, 12, 8)
            };
            filterPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            filterPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            filterPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            filterPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

            var startLabel = new Label {
                Text = "絞込 開始日",
                Dock = DockStyle.Fill,
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft
            };
            FFilterStartDatePicker = new DateTimePicker {
                Dock = DockStyle.Fill,
                Format = DateTimePickerFormat.Short,
                ShowCheckBox = true,
                Checked = false
            };
            filterPanel.Controls.Add(startLabel, 0, 0);
            filterPanel.Controls.Add(FFilterStartDatePicker, 1, 0);

            var endLabel = new Label {
                Text = "絞込 終了日",
                Dock = DockStyle.Fill,
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft
            };
            FFilterEndDatePicker = new DateTimePicker {
                Dock = DockStyle.Fill,
                Format = DateTimePickerFormat.Short,
                ShowCheckBox = true,
                Checked = false
            };
            filterPanel.Controls.Add(endLabel, 2, 0);
            filterPanel.Controls.Add(FFilterEndDatePicker, 3, 0);

            FFilterStoreIdText = AddLabeledTextBox(filterPanel, "絞込 店舗ID", 0, 1);
            FFilterProductIdText = AddLabeledTextBox(filterPanel, "絞込 商品ID", 2, 1);

            var filterButtons = new FlowLayoutPanel {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = true
            };
            filterButtons.Controls.Add(CreateButton("絞り込み", delegate { FilterRequested?.Invoke(this, EventArgs.Empty); }));
            filterButtons.Controls.Add(CreateButton("解除", delegate { FilterClearRequested?.Invoke(this, EventArgs.Empty); }));
            filterPanel.Controls.Add(filterButtons, 0, 2);
            filterPanel.SetColumnSpan(filterButtons, 4);

            var hintLabel = new Label {
                Dock = DockStyle.Fill,
                Padding = new Padding(12, 0, 0, 0),
                Text = "商品選択で単価を表示します。数量は整数で入力してください。",
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft
            };

            FGrid = new DataGridView {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AutoGenerateColumns = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false
            };

            FStoreIdText.TextChanged += delegate { InputChanged?.Invoke(this, EventArgs.Empty); };
            FQuantityText.TextChanged += delegate { InputChanged?.Invoke(this, EventArgs.Empty); };
            FProductCombo.SelectedIndexChanged += delegate { InputChanged?.Invoke(this, EventArgs.Empty); };

            root.Controls.Add(inputPanel, 0, 0);
            root.Controls.Add(filterPanel, 0, 1);
            root.Controls.Add(hintLabel, 0, 2);
            root.Controls.Add(FGrid, 0, 3);
            Controls.Add(root);
        }

        internal SalesInputModel GetInput() {
            var selected = FProductCombo.SelectedItem as SaleProductOption;
            return new SalesInputModel {
                SaleDate = FSaleDatePicker.Value.Date,
                StoreId = FStoreIdText.Text.Trim(),
                ProductId = selected == null ? string.Empty : selected.ProductId,
                QuantityText = FQuantityText.Text.Trim()
            };
        }

        internal SalesFilterModel GetFilter() {
            return new SalesFilterModel {
                StartDate = FFilterStartDatePicker.Checked ? FFilterStartDatePicker.Value.Date : (DateTime?)null,
                EndDate = FFilterEndDatePicker.Checked ? FFilterEndDatePicker.Value.Date : (DateTime?)null,
                StoreId = FFilterStoreIdText.Text.Trim(),
                ProductId = FFilterProductIdText.Text.Trim()
            };
        }

        internal SaleProductOption GetSelectedProductOption() {
            return FProductCombo.SelectedItem as SaleProductOption;
        }

        internal void SetProductOptions(IReadOnlyCollection<SaleProductOption> options, string selectedProductId) {
            var selectedId = string.IsNullOrWhiteSpace(selectedProductId) ? string.Empty : selectedProductId;
            FProductCombo.DataSource = null;
            FProductCombo.DisplayMember = "DisplayText";
            FProductCombo.ValueMember = "ProductId";
            FProductCombo.DataSource = options.ToList();

            if (!string.IsNullOrWhiteSpace(selectedId)) {
                FProductCombo.SelectedValue = selectedId;
            }

            if (FProductCombo.Items.Count == 0) {
                FProductCombo.SelectedIndex = -1;
            }
        }

        internal void SetRows(IReadOnlyCollection<SalesViewRow> rows) {
            FGrid.DataSource = null;
            FGrid.DataSource = rows.ToList();
            DataGridHeaderMapper.Apply(FGrid, FHeaderMap);
        }

        internal void ClearInput() {
            FSaleDatePicker.Value = DateTime.Today;
            FStoreIdText.Text = string.Empty;
            FQuantityText.Text = string.Empty;
            if (FProductCombo.Items.Count > 0) {
                FProductCombo.SelectedIndex = 0;
            }
        }

        internal void ClearFilter() {
            FFilterStartDatePicker.Checked = false;
            FFilterEndDatePicker.Checked = false;
            FFilterStoreIdText.Text = string.Empty;
            FFilterProductIdText.Text = string.Empty;
        }

        internal void SetUnitPriceLabel(string text) {
            FUnitPriceLabel.Text = text;
        }

        internal void SetAmountPreviewLabel(string text) {
            FAmountPreviewLabel.Text = text;
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
