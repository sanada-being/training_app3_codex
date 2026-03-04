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

            var wRoot = new TableLayoutPanel {
                Dock = DockStyle.Fill,
                RowCount = 3,
                ColumnCount = 1
            };
            wRoot.RowStyles.Add(new RowStyle(SizeType.Absolute, 154));
            wRoot.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
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

            var wStartLabel = new Label {
                Text = "開始日",
                Dock = DockStyle.Fill,
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft
            };
            FStartDatePicker = new DateTimePicker {
                Dock = DockStyle.Fill,
                Format = DateTimePickerFormat.Short,
                Value = DateTime.Today.AddDays(-6)
            };
            wInputPanel.Controls.Add(wStartLabel, 0, 0);
            wInputPanel.Controls.Add(FStartDatePicker, 1, 0);

            var wEndLabel = new Label {
                Text = "終了日",
                Dock = DockStyle.Fill,
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft
            };
            FEndDatePicker = new DateTimePicker {
                Dock = DockStyle.Fill,
                Format = DateTimePickerFormat.Short,
                Value = DateTime.Today
            };
            wInputPanel.Controls.Add(wEndLabel, 2, 0);
            wInputPanel.Controls.Add(FEndDatePicker, 3, 0);

            var wButtonFlow = new FlowLayoutPanel {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = true
            };
            wButtonFlow.Controls.Add(CreateButton("集計実行", delegate { ExecuteRequested?.Invoke(this, EventArgs.Empty); }));
            wButtonFlow.Controls.Add(CreateButton("集計結果コピー", delegate { CopyRequested?.Invoke(this, EventArgs.Empty); }));
            wInputPanel.Controls.Add(wButtonFlow, 0, 1);
            wInputPanel.SetColumnSpan(wButtonFlow, 4);

            FProductIdFilterText = AddLabeledTextBox(wInputPanel, "絞込 商品ID", 0, 2);
            var wFilterButtons = new FlowLayoutPanel {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = true
            };
            wFilterButtons.Controls.Add(CreateButton("絞り込み", delegate { FilterRequested?.Invoke(this, EventArgs.Empty); }));
            wFilterButtons.Controls.Add(CreateButton("解除", delegate { FilterClearRequested?.Invoke(this, EventArgs.Empty); }));
            wInputPanel.Controls.Add(wFilterButtons, 2, 2);
            wInputPanel.SetColumnSpan(wFilterButtons, 2);

            FSummaryTotalLabel = new Label {
                Dock = DockStyle.Fill,
                Padding = new Padding(12, 0, 0, 0),
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft
            };

            var wSplit = new SplitContainer {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Horizontal,
                SplitterDistance = 200
            };

            var wProductPanel = new TableLayoutPanel {
                Dock = DockStyle.Fill,
                RowCount = 2,
                ColumnCount = 1
            };
            wProductPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
            wProductPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            wProductPanel.Controls.Add(new Label {
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
            wProductPanel.Controls.Add(FProductSummaryGrid, 0, 1);

            var wWeeklyPanel = new TableLayoutPanel {
                Dock = DockStyle.Fill,
                RowCount = 2,
                ColumnCount = 1
            };
            wWeeklyPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
            wWeeklyPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            wWeeklyPanel.Controls.Add(new Label {
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
            wWeeklyPanel.Controls.Add(FWeeklySummaryGrid, 0, 1);

            wSplit.Panel1.Controls.Add(wProductPanel);
            wSplit.Panel2.Controls.Add(wWeeklyPanel);

            wRoot.Controls.Add(wInputPanel, 0, 0);
            wRoot.Controls.Add(FSummaryTotalLabel, 0, 1);
            wRoot.Controls.Add(wSplit, 0, 2);
            Controls.Add(wRoot);
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

        internal void SetSummaryText(string vText) {
            FSummaryTotalLabel.Text = vText;
        }

        internal void SetProductRows(IReadOnlyCollection<AggregationProductSummaryRow> vRows) {
            FProductSummaryGrid.DataSource = null;
            FProductSummaryGrid.DataSource = vRows.ToList();
            DataGridHeaderMapper.Apply(FProductSummaryGrid, FProductSummaryHeaderMap);
        }

        internal void SetWeeklyRows(IReadOnlyCollection<AggregationWeeklySummaryRow> vRows) {
            FWeeklySummaryGrid.DataSource = null;
            FWeeklySummaryGrid.DataSource = vRows.ToList();
            DataGridHeaderMapper.Apply(FWeeklySummaryGrid, FWeeklySummaryHeaderMap);
        }

        internal void ResetDisplay() {
            SetSummaryText("期間を指定して集計を実行してください。");
            SetProductRows(new List<AggregationProductSummaryRow>());
            SetWeeklyRows(new List<AggregationWeeklySummaryRow>());
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
