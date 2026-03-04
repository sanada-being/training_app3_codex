using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using SalesManagementApp.Core.Domain.Entities;
using Sales_Management_App.Presentation.Common;

namespace Sales_Management_App.Presentation.Tabs.InventoryHistory {
    /// <summary>
    /// 在庫履歴 タブのUI構成と表示更新を担当するビューです。
    /// </summary>
    internal sealed class InventoryHistoryView : UserControl {
        private static readonly IReadOnlyDictionary<string, string> FHeaderMap =
            new Dictionary<string, string> {
                { "OccurredAt", "日時" },
                { "OperationType", "操作種別" },
                { "StoreId", "店舗ID" },
                { "ProductId", "商品ID" },
                { "Quantity", "数量" },
                { "ResultStock", "実行後在庫" },
                { "Result", "実行結果" }
            };

        private readonly DataGridView FGrid;
        private readonly DateTimePicker FStartDatePicker;
        private readonly DateTimePicker FEndDatePicker;
        private readonly ComboBox FOperationTypeCombo;
        private readonly TextBox FStoreIdText;
        private readonly TextBox FProductIdText;

        internal event EventHandler FilterRequested;
        internal event EventHandler FilterClearRequested;

        internal InventoryHistoryView() {
            Dock = DockStyle.Fill;

            var wRoot = new TableLayoutPanel {
                Dock = DockStyle.Fill,
                RowCount = 2,
                ColumnCount = 1
            };
            wRoot.RowStyles.Add(new RowStyle(SizeType.Absolute, 160));
            wRoot.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            var wFilterPanel = new TableLayoutPanel {
                Dock = DockStyle.Fill,
                ColumnCount = 6,
                RowCount = 3,
                Padding = new Padding(12)
            };
            wFilterPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            wFilterPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            wFilterPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            wFilterPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            wFilterPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            wFilterPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 220));

            var wStartLabel = new Label {
                Text = "開始日時",
                Dock = DockStyle.Fill,
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft
            };
            FStartDatePicker = new DateTimePicker {
                Dock = DockStyle.Fill,
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "yyyy/MM/dd HH:mm:ss",
                ShowCheckBox = true,
                Checked = false
            };
            wFilterPanel.Controls.Add(wStartLabel, 0, 0);
            wFilterPanel.Controls.Add(FStartDatePicker, 1, 0);

            var wEndLabel = new Label {
                Text = "終了日時",
                Dock = DockStyle.Fill,
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft
            };
            FEndDatePicker = new DateTimePicker {
                Dock = DockStyle.Fill,
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "yyyy/MM/dd HH:mm:ss",
                ShowCheckBox = true,
                Checked = false
            };
            wFilterPanel.Controls.Add(wEndLabel, 2, 0);
            wFilterPanel.Controls.Add(FEndDatePicker, 3, 0);

            var wOperationLabel = new Label {
                Text = "操作種別",
                Dock = DockStyle.Fill,
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft
            };
            FOperationTypeCombo = new ComboBox {
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            wFilterPanel.Controls.Add(wOperationLabel, 4, 0);
            wFilterPanel.Controls.Add(FOperationTypeCombo, 5, 0);

            FStoreIdText = AddLabeledTextBox(wFilterPanel, "店舗ID", 0, 1);
            FProductIdText = AddLabeledTextBox(wFilterPanel, "商品ID", 2, 1);

            var wButtonFlow = new FlowLayoutPanel {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = true
            };
            wButtonFlow.Controls.Add(CreateButton("検索", delegate { FilterRequested?.Invoke(this, EventArgs.Empty); }));
            wButtonFlow.Controls.Add(CreateButton("クリア", delegate { FilterClearRequested?.Invoke(this, EventArgs.Empty); }));
            wFilterPanel.Controls.Add(wButtonFlow, 0, 2);
            wFilterPanel.SetColumnSpan(wButtonFlow, 6);

            FGrid = new DataGridView {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AutoGenerateColumns = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false
            };

            wRoot.Controls.Add(wFilterPanel, 0, 0);
            wRoot.Controls.Add(FGrid, 0, 1);
            Controls.Add(wRoot);

            BindOperationOptions();
        }

        internal InventoryHistoryFilterModel GetFilter() {
            return new InventoryHistoryFilterModel {
                StartDateTime = FStartDatePicker.Checked ? FStartDatePicker.Value : (DateTime?)null,
                EndDateTime = FEndDatePicker.Checked ? FEndDatePicker.Value : (DateTime?)null,
                StoreId = FStoreIdText.Text.Trim(),
                ProductId = FProductIdText.Text.Trim(),
                OperationType = GetSelectedOperationType()
            };
        }

        internal void SetRows(IReadOnlyCollection<InventoryHistoryViewRow> vRows) {
            FGrid.DataSource = null;
            FGrid.DataSource = vRows.ToList();
            DataGridHeaderMapper.Apply(FGrid, FHeaderMap);
        }

        internal void ClearFilter() {
            FStartDatePicker.Checked = false;
            FEndDatePicker.Checked = false;
            FStoreIdText.Text = string.Empty;
            FProductIdText.Text = string.Empty;
            FOperationTypeCombo.SelectedIndex = 0;
        }

        private void BindOperationOptions() {
            var wOptions = new List<InventoryHistoryOperationFilterOption> {
                new InventoryHistoryOperationFilterOption(string.Empty, "すべて"),
                new InventoryHistoryOperationFilterOption(InventoryOperationTypeEnum.Inbound.ToString(), "入荷"),
                new InventoryHistoryOperationFilterOption(InventoryOperationTypeEnum.Outbound.ToString(), "出庫"),
                new InventoryHistoryOperationFilterOption(InventoryOperationTypeEnum.Sale.ToString(), "売上連動")
            };

            FOperationTypeCombo.DataSource = null;
            FOperationTypeCombo.DisplayMember = "Label";
            FOperationTypeCombo.ValueMember = "Value";
            FOperationTypeCombo.DataSource = wOptions;
            FOperationTypeCombo.SelectedIndex = 0;
        }

        private InventoryOperationTypeEnum? GetSelectedOperationType() {
            var wOption = FOperationTypeCombo.SelectedItem as InventoryHistoryOperationFilterOption;
            if (wOption == null || string.IsNullOrWhiteSpace(wOption.Value)) {
                return null;
            }

            InventoryOperationTypeEnum wParsed;
            return Enum.TryParse(wOption.Value, out wParsed) ? wParsed : (InventoryOperationTypeEnum?)null;
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

