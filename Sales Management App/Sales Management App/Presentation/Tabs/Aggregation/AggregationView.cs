using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Sales_Management_App.Presentation.Common;

namespace Sales_Management_App.Presentation.Tabs.Aggregation {
    /// <summary>
    /// 売上集計 タブのUI構成と表示更新を担当するビューです。
    /// </summary>
    internal sealed class AggregationView : UserControl {
        private static readonly IReadOnlyDictionary<string, string> FProductSummaryHeaderMap =
            new Dictionary<string, string> {
                { "ProductId", "商品ID" },
                { "TotalQuantity", "販売数量" },
                { "TotalSalesAmount", "売上金額" }
            };

        private static readonly IReadOnlyDictionary<string, string> FWeeklySummaryHeaderMap =
            new Dictionary<string, string> {
                { "Week", "週" },
                { "TotalQuantity", "販売数量" },
                { "TotalSalesAmount", "売上金額" }
            };

        private readonly DateTimePicker FStartDatePicker;
        private readonly DateTimePicker FEndDatePicker;
        private readonly Label FSummaryTotalLabel;
        private readonly TextBox FProductIdFilterText;
        private readonly DataGridView FProductSummaryGrid;
        private readonly DataGridView FWeeklySummaryGrid;

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
            FStartDatePicker = new DateTimePicker {
                Dock = DockStyle.Fill,
                Format = DateTimePickerFormat.Short,
                Value = DateTime.Today.AddDays(-6)
            };
            inputPanel.Controls.Add(startLabel, 0, 0);
            inputPanel.Controls.Add(FStartDatePicker, 1, 0);

            var endLabel = new Label {
                Text = "終了日",
                Dock = DockStyle.Fill,
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft
            };
            FEndDatePicker = new DateTimePicker {
                Dock = DockStyle.Fill,
                Format = DateTimePickerFormat.Short,
                Value = DateTime.Today
            };
            inputPanel.Controls.Add(endLabel, 2, 0);
            inputPanel.Controls.Add(FEndDatePicker, 3, 0);

            var buttonFlow = new FlowLayoutPanel {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = true
            };
            buttonFlow.Controls.Add(CreateButton("集計実行", delegate { ExecuteRequested?.Invoke(this, EventArgs.Empty); }));
            buttonFlow.Controls.Add(CreateButton("集計結果コピー", delegate { CopyRequested?.Invoke(this, EventArgs.Empty); }));
            inputPanel.Controls.Add(buttonFlow, 0, 1);
            inputPanel.SetColumnSpan(buttonFlow, 4);

            FProductIdFilterText = AddLabeledTextBox(inputPanel, "絞込 商品ID", 0, 2);
            var filterButtons = new FlowLayoutPanel {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = true
            };
            filterButtons.Controls.Add(CreateButton("絞り込み", delegate { FilterRequested?.Invoke(this, EventArgs.Empty); }));
            filterButtons.Controls.Add(CreateButton("解除", delegate { FilterClearRequested?.Invoke(this, EventArgs.Empty); }));
            inputPanel.Controls.Add(filterButtons, 2, 2);
            inputPanel.SetColumnSpan(filterButtons, 2);

            FSummaryTotalLabel = new Label {
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

            FProductSummaryGrid = new DataGridView {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AutoGenerateColumns = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false
            };
            productPanel.Controls.Add(FProductSummaryGrid, 0, 1);

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

            FWeeklySummaryGrid = new DataGridView {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AutoGenerateColumns = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false
            };
            weeklyPanel.Controls.Add(FWeeklySummaryGrid, 0, 1);

            split.Panel1.Controls.Add(productPanel);
            split.Panel2.Controls.Add(weeklyPanel);

            root.Controls.Add(inputPanel, 0, 0);
            root.Controls.Add(FSummaryTotalLabel, 0, 1);
            root.Controls.Add(split, 0, 2);
            Controls.Add(root);
        }

        internal DateTime GetStartDate() {
            return FStartDatePicker.Value.Date;
        }

        internal DateTime GetEndDate() {
            return FEndDatePicker.Value.Date;
        }

        internal string GetProductIdFilter() {
            return FProductIdFilterText.Text.Trim();
        }

        internal void ClearProductIdFilter() {
            FProductIdFilterText.Text = string.Empty;
        }

        internal void SetSummaryText(string text) {
            FSummaryTotalLabel.Text = text;
        }

        internal void SetProductRows(IReadOnlyCollection<AggregationProductSummaryRow> rows) {
            FProductSummaryGrid.DataSource = null;
            FProductSummaryGrid.DataSource = rows.ToList();
            DataGridHeaderMapper.Apply(FProductSummaryGrid, FProductSummaryHeaderMap);
        }

        internal void SetWeeklyRows(IReadOnlyCollection<AggregationWeeklySummaryRow> rows) {
            FWeeklySummaryGrid.DataSource = null;
            FWeeklySummaryGrid.DataSource = rows.ToList();
            DataGridHeaderMapper.Apply(FWeeklySummaryGrid, FWeeklySummaryHeaderMap);
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
