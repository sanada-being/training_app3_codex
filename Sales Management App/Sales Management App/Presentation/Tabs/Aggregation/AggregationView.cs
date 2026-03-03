using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Sales_Management_App.Presentation.Common;

namespace Sales_Management_App.Presentation.Tabs.Aggregation {
    internal sealed class AggregationView : UserControl {
        private static readonly IReadOnlyDictionary<string, string> ProductSummaryHeaderMap =
            new Dictionary<string, string> {
                { "ProductId", "商品ID" },
                { "TotalQuantity", "販売数量" },
                { "TotalSalesAmount", "売上金額" }
            };

        private static readonly IReadOnlyDictionary<string, string> WeeklySummaryHeaderMap =
            new Dictionary<string, string> {
                { "Week", "週" },
                { "TotalQuantity", "販売数量" },
                { "TotalSalesAmount", "売上金額" }
            };

        private readonly DateTimePicker _startDatePicker;
        private readonly DateTimePicker _endDatePicker;
        private readonly Label _summaryTotalLabel;
        private readonly TextBox _productIdFilterText;
        private readonly DataGridView _productSummaryGrid;
        private readonly DataGridView _weeklySummaryGrid;

        internal event EventHandler ExecuteRequested;
        internal event EventHandler CopyRequested;
        internal event EventHandler FilterRequested;
        internal event EventHandler FilterClearRequested;

        internal AggregationView() {
            Dock = DockStyle.Fill;

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
            _startDatePicker = new DateTimePicker {
                Dock = DockStyle.Fill,
                Format = DateTimePickerFormat.Short,
                Value = DateTime.Today.AddDays(-6)
            };
            inputPanel.Controls.Add(startLabel, 0, 0);
            inputPanel.Controls.Add(_startDatePicker, 1, 0);

            var endLabel = new Label {
                Text = "終了日",
                Dock = DockStyle.Fill,
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft
            };
            _endDatePicker = new DateTimePicker {
                Dock = DockStyle.Fill,
                Format = DateTimePickerFormat.Short,
                Value = DateTime.Today
            };
            inputPanel.Controls.Add(endLabel, 2, 0);
            inputPanel.Controls.Add(_endDatePicker, 3, 0);

            var buttonFlow = new FlowLayoutPanel {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = true
            };
            buttonFlow.Controls.Add(CreateButton("集計実行", delegate { ExecuteRequested?.Invoke(this, EventArgs.Empty); }));
            buttonFlow.Controls.Add(CreateButton("集計結果コピー", delegate { CopyRequested?.Invoke(this, EventArgs.Empty); }));
            inputPanel.Controls.Add(buttonFlow, 0, 1);
            inputPanel.SetColumnSpan(buttonFlow, 4);

            _productIdFilterText = AddLabeledTextBox(inputPanel, "絞込 商品ID", 0, 2);
            var filterButtons = new FlowLayoutPanel {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = true
            };
            filterButtons.Controls.Add(CreateButton("絞り込み", delegate { FilterRequested?.Invoke(this, EventArgs.Empty); }));
            filterButtons.Controls.Add(CreateButton("解除", delegate { FilterClearRequested?.Invoke(this, EventArgs.Empty); }));
            inputPanel.Controls.Add(filterButtons, 2, 2);
            inputPanel.SetColumnSpan(filterButtons, 2);

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
            Controls.Add(root);
        }

        internal DateTime GetStartDate() {
            return _startDatePicker.Value.Date;
        }

        internal DateTime GetEndDate() {
            return _endDatePicker.Value.Date;
        }

        internal string GetProductIdFilter() {
            return _productIdFilterText.Text.Trim();
        }

        internal void ClearProductIdFilter() {
            _productIdFilterText.Text = string.Empty;
        }

        internal void SetSummaryText(string text) {
            _summaryTotalLabel.Text = text;
        }

        internal void SetProductRows(IReadOnlyCollection<AggregationProductSummaryRow> rows) {
            _productSummaryGrid.DataSource = null;
            _productSummaryGrid.DataSource = rows.ToList();
            DataGridHeaderMapper.Apply(_productSummaryGrid, ProductSummaryHeaderMap);
        }

        internal void SetWeeklyRows(IReadOnlyCollection<AggregationWeeklySummaryRow> rows) {
            _weeklySummaryGrid.DataSource = null;
            _weeklySummaryGrid.DataSource = rows.ToList();
            DataGridHeaderMapper.Apply(_weeklySummaryGrid, WeeklySummaryHeaderMap);
        }

        internal void ResetDisplay() {
            SetSummaryText("期間を指定して集計を実行してください。");
            SetProductRows(new List<AggregationProductSummaryRow>());
            SetWeeklyRows(new List<AggregationWeeklySummaryRow>());
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
