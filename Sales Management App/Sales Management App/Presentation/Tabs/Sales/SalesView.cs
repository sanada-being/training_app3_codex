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

            var wRoot = new TableLayoutPanel {
                Dock = DockStyle.Fill,
                RowCount = 4,
                ColumnCount = 1
            };
            wRoot.RowStyles.Add(new RowStyle(SizeType.Absolute, 200));
            wRoot.RowStyles.Add(new RowStyle(SizeType.Absolute, 120));
            wRoot.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
            wRoot.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            var wInputPanel = new TableLayoutPanel {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 4,
                Padding = new Padding(12)
            };
            wInputPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            wInputPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            wInputPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            wInputPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

            var wSaleDateLabel = new Label {
                Text = "販売日",
                Dock = DockStyle.Fill,
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft
            };
            FSaleDatePicker = new DateTimePicker {
                Dock = DockStyle.Fill,
                Format = DateTimePickerFormat.Short
            };
            wInputPanel.Controls.Add(wSaleDateLabel, 0, 0);
            wInputPanel.Controls.Add(FSaleDatePicker, 1, 0);

            FStoreIdText = AddLabeledTextBox(wInputPanel, "店舗ID", 2, 0);

            var wProductLabel = new Label {
                Text = "商品",
                Dock = DockStyle.Fill,
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft
            };
            FProductCombo = new ComboBox {
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            wInputPanel.Controls.Add(wProductLabel, 0, 1);
            wInputPanel.Controls.Add(FProductCombo, 1, 1);

            FQuantityText = AddLabeledTextBox(wInputPanel, "数量", 2, 1);

            var wUnitPriceHeaderLabel = new Label {
                Text = "単価",
                Dock = DockStyle.Fill,
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft
            };
            FUnitPriceLabel = new Label {
                Dock = DockStyle.Fill,
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft
            };
            wInputPanel.Controls.Add(wUnitPriceHeaderLabel, 0, 2);
            wInputPanel.Controls.Add(FUnitPriceLabel, 1, 2);

            var wAmountHeaderLabel = new Label {
                Text = "売上金額見込",
                Dock = DockStyle.Fill,
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft
            };
            FAmountPreviewLabel = new Label {
                Dock = DockStyle.Fill,
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft
            };
            wInputPanel.Controls.Add(wAmountHeaderLabel, 2, 2);
            wInputPanel.Controls.Add(FAmountPreviewLabel, 3, 2);

            var wButtonFlow = new FlowLayoutPanel {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = true
            };
            wButtonFlow.Controls.Add(CreateButton("売上登録", delegate { RegisterRequested?.Invoke(this, EventArgs.Empty); }));
            wButtonFlow.Controls.Add(CreateButton("クリア", delegate { ClearInputRequested?.Invoke(this, EventArgs.Empty); }));
            wInputPanel.Controls.Add(wButtonFlow, 0, 3);
            wInputPanel.SetColumnSpan(wButtonFlow, 4);

            var wFilterPanel = new TableLayoutPanel {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 3,
                Padding = new Padding(12, 0, 12, 8)
            };
            wFilterPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            wFilterPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            wFilterPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            wFilterPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

            var wStartLabel = new Label {
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
            wFilterPanel.Controls.Add(wStartLabel, 0, 0);
            wFilterPanel.Controls.Add(FFilterStartDatePicker, 1, 0);

            var wEndLabel = new Label {
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
            wFilterPanel.Controls.Add(wEndLabel, 2, 0);
            wFilterPanel.Controls.Add(FFilterEndDatePicker, 3, 0);

            FFilterStoreIdText = AddLabeledTextBox(wFilterPanel, "絞込 店舗ID", 0, 1);
            FFilterProductIdText = AddLabeledTextBox(wFilterPanel, "絞込 商品ID", 2, 1);

            var wFilterButtons = new FlowLayoutPanel {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = true
            };
            wFilterButtons.Controls.Add(CreateButton("絞り込み", delegate { FilterRequested?.Invoke(this, EventArgs.Empty); }));
            wFilterButtons.Controls.Add(CreateButton("解除", delegate { FilterClearRequested?.Invoke(this, EventArgs.Empty); }));
            wFilterPanel.Controls.Add(wFilterButtons, 0, 2);
            wFilterPanel.SetColumnSpan(wFilterButtons, 4);

            var wHintLabel = new Label {
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

            wRoot.Controls.Add(wInputPanel, 0, 0);
            wRoot.Controls.Add(wFilterPanel, 0, 1);
            wRoot.Controls.Add(wHintLabel, 0, 2);
            wRoot.Controls.Add(FGrid, 0, 3);
            Controls.Add(wRoot);
        }

        internal SalesInputModel GetInput() {
            var wSelected = FProductCombo.SelectedItem as SaleProductOption;
            return new SalesInputModel {
                SaleDate = FSaleDatePicker.Value.Date,
                StoreId = FStoreIdText.Text.Trim(),
                ProductId = wSelected == null ? string.Empty : wSelected.ProductId,
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

        internal void SetProductOptions(IReadOnlyCollection<SaleProductOption> vOptions, string vSelectedProductId) {
            var wSelectedId = string.IsNullOrWhiteSpace(vSelectedProductId) ? string.Empty : vSelectedProductId;
            FProductCombo.DataSource = null;
            FProductCombo.DisplayMember = "DisplayText";
            FProductCombo.ValueMember = "ProductId";
            FProductCombo.DataSource = vOptions.ToList();

            if (!string.IsNullOrWhiteSpace(wSelectedId)) {
                FProductCombo.SelectedValue = wSelectedId;
            }

            if (FProductCombo.Items.Count == 0) {
                FProductCombo.SelectedIndex = -1;
            }
        }

        internal void SetRows(IReadOnlyCollection<SalesViewRow> vRows) {
            FGrid.DataSource = null;
            FGrid.DataSource = vRows.ToList();
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

        internal void SetUnitPriceLabel(string vText) {
            FUnitPriceLabel.Text = vText;
        }

        internal void SetAmountPreviewLabel(string vText) {
            FAmountPreviewLabel.Text = vText;
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
