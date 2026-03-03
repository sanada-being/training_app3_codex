using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using SalesManagementApp.Core.Domain.Entities;
using Sales_Management_App.Presentation.Common;
using InventoryHistoryOperationFilterOption = Sales_Management_App.InventoryHistoryOperationFilterOption;

namespace Sales_Management_App.Presentation.Tabs.InventoryHistory {
    internal sealed class InventoryHistoryView : UserControl {
        private static readonly IReadOnlyDictionary<string, string> HeaderMap =
            new Dictionary<string, string> {
                { "OccurredAt", "日時" },
                { "OperationType", "操作種別" },
                { "StoreId", "店舗ID" },
                { "ProductId", "商品ID" },
                { "Quantity", "数量" },
                { "ResultStock", "実行後在庫" },
                { "Result", "実行結果" }
            };

        private readonly DataGridView _grid;
        private readonly DateTimePicker _startDatePicker;
        private readonly DateTimePicker _endDatePicker;
        private readonly ComboBox _operationTypeCombo;
        private readonly TextBox _storeIdText;
        private readonly TextBox _productIdText;

        internal event EventHandler FilterRequested;
        internal event EventHandler FilterClearRequested;

        internal InventoryHistoryView() {
            Dock = DockStyle.Fill;

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
            _startDatePicker = new DateTimePicker {
                Dock = DockStyle.Fill,
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "yyyy/MM/dd HH:mm:ss",
                ShowCheckBox = true,
                Checked = false
            };
            filterPanel.Controls.Add(startLabel, 0, 0);
            filterPanel.Controls.Add(_startDatePicker, 1, 0);

            var endLabel = new Label {
                Text = "終了日時",
                Dock = DockStyle.Fill,
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft
            };
            _endDatePicker = new DateTimePicker {
                Dock = DockStyle.Fill,
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "yyyy/MM/dd HH:mm:ss",
                ShowCheckBox = true,
                Checked = false
            };
            filterPanel.Controls.Add(endLabel, 2, 0);
            filterPanel.Controls.Add(_endDatePicker, 3, 0);

            var operationLabel = new Label {
                Text = "操作種別",
                Dock = DockStyle.Fill,
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft
            };
            _operationTypeCombo = new ComboBox {
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            filterPanel.Controls.Add(operationLabel, 4, 0);
            filterPanel.Controls.Add(_operationTypeCombo, 5, 0);

            _storeIdText = AddLabeledTextBox(filterPanel, "店舗ID", 0, 1);
            _productIdText = AddLabeledTextBox(filterPanel, "商品ID", 2, 1);

            var buttonFlow = new FlowLayoutPanel {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = true
            };
            buttonFlow.Controls.Add(CreateButton("検索", delegate { FilterRequested?.Invoke(this, EventArgs.Empty); }));
            buttonFlow.Controls.Add(CreateButton("クリア", delegate { FilterClearRequested?.Invoke(this, EventArgs.Empty); }));
            filterPanel.Controls.Add(buttonFlow, 0, 2);
            filterPanel.SetColumnSpan(buttonFlow, 6);

            _grid = new DataGridView {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AutoGenerateColumns = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false
            };

            root.Controls.Add(filterPanel, 0, 0);
            root.Controls.Add(_grid, 0, 1);
            Controls.Add(root);

            BindOperationOptions();
        }

        internal InventoryHistoryFilterModel GetFilter() {
            return new InventoryHistoryFilterModel {
                StartDateTime = _startDatePicker.Checked ? _startDatePicker.Value : (DateTime?)null,
                EndDateTime = _endDatePicker.Checked ? _endDatePicker.Value : (DateTime?)null,
                StoreId = _storeIdText.Text.Trim(),
                ProductId = _productIdText.Text.Trim(),
                OperationType = GetSelectedOperationType()
            };
        }

        internal void SetRows(IReadOnlyCollection<InventoryHistoryViewRow> rows) {
            _grid.DataSource = null;
            _grid.DataSource = rows.ToList();
            DataGridHeaderMapper.Apply(_grid, HeaderMap);
        }

        internal void ClearFilter() {
            _startDatePicker.Checked = false;
            _endDatePicker.Checked = false;
            _storeIdText.Text = string.Empty;
            _productIdText.Text = string.Empty;
            _operationTypeCombo.SelectedIndex = 0;
        }

        private void BindOperationOptions() {
            var options = new List<InventoryHistoryOperationFilterOption> {
                new InventoryHistoryOperationFilterOption(string.Empty, "すべて"),
                new InventoryHistoryOperationFilterOption(InventoryOperationType.Inbound.ToString(), "入荷"),
                new InventoryHistoryOperationFilterOption(InventoryOperationType.Outbound.ToString(), "出庫"),
                new InventoryHistoryOperationFilterOption(InventoryOperationType.Sale.ToString(), "売上連動")
            };

            _operationTypeCombo.DataSource = null;
            _operationTypeCombo.DisplayMember = "Label";
            _operationTypeCombo.ValueMember = "Value";
            _operationTypeCombo.DataSource = options;
            _operationTypeCombo.SelectedIndex = 0;
        }

        private InventoryOperationType? GetSelectedOperationType() {
            var option = _operationTypeCombo.SelectedItem as InventoryHistoryOperationFilterOption;
            if (option == null || string.IsNullOrWhiteSpace(option.Value)) {
                return null;
            }

            InventoryOperationType parsed;
            return Enum.TryParse(option.Value, out parsed) ? parsed : (InventoryOperationType?)null;
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
