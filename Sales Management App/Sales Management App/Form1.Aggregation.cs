using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Sales_Management_App {
    public partial class Form1 {
        private void ExecuteAggregation(object sender, EventArgs e) {
            ExecuteWithValidation(delegate {
                var snapshot = BuildAggregationSnapshot();
                RenderAggregationSnapshot(snapshot);
            });
        }

        private void CopyAggregationResult(object sender, EventArgs e) {
            ExecuteWithValidation(delegate {
                var snapshot = BuildAggregationSnapshot();
                RenderAggregationSnapshot(snapshot);
                Clipboard.SetText(BuildAggregationClipboardText(snapshot));
                MessageBox.Show("集計結果をクリップボードにコピーしました。", "完了", MessageBoxButtons.OK, MessageBoxIcon.Information);
            });
        }

        private AggregationSnapshot BuildAggregationSnapshot() {
            var startDate = _summaryStartDatePicker.Value.Date;
            var endDate = _summaryEndDatePicker.Value.Date;

            return new AggregationSnapshot {
                StartDate = startDate,
                EndDate = endDate,
                ProductSummaries = _salesAggregationService.GetProductSummaries(_sales, startDate, endDate).ToList(),
                WeeklySummaries = _salesAggregationService.GetWeeklySummaries(_sales, startDate, endDate).ToList(),
                TotalSalesAmount = _salesAggregationService.GetTotalSalesAmount(_sales, startDate, endDate)
            };
        }

        private void RenderAggregationSnapshot(AggregationSnapshot snapshot) {
            _summaryTotalLabel.Text = string.Format(
                "期間: {0:yyyy/MM/dd} - {1:yyyy/MM/dd} / 合計売上: {2} 円",
                snapshot.StartDate,
                snapshot.EndDate,
                snapshot.TotalSalesAmount);

            _productSummaryGrid.DataSource = null;
            _productSummaryGrid.DataSource = snapshot.ProductSummaries.Select(s => new {
                s.ProductId,
                s.TotalQuantity,
                s.TotalSalesAmount
            }).ToList();

            _weeklySummaryGrid.DataSource = null;
            _weeklySummaryGrid.DataSource = snapshot.WeeklySummaries.Select(s => new {
                Week = string.Format("{0:yyyy/MM/dd} - {1:yyyy/MM/dd}", s.WeekStartDate, s.WeekEndDate),
                s.TotalQuantity,
                s.TotalSalesAmount
            }).ToList();
        }

        private void ResetAggregationDisplay() {
            _summaryTotalLabel.Text = "期間を指定して集計を実行してください。";
            _productSummaryGrid.DataSource = null;
            _productSummaryGrid.DataSource = new List<object>();
            _weeklySummaryGrid.DataSource = null;
            _weeklySummaryGrid.DataSource = new List<object>();
        }

        private static string BuildAggregationClipboardText(AggregationSnapshot snapshot) {
            var builder = new StringBuilder();
            builder.AppendLine(string.Format("期間: {0:yyyy/MM/dd} - {1:yyyy/MM/dd}", snapshot.StartDate, snapshot.EndDate));
            builder.AppendLine(string.Format("合計売上: {0} 円", snapshot.TotalSalesAmount));
            builder.AppendLine();
            builder.AppendLine("[商品別集計]");
            builder.AppendLine("商品ID\t販売数量\t売上金額");

            foreach (var summary in snapshot.ProductSummaries) {
                builder.AppendLine(string.Format("{0}\t{1}\t{2}", summary.ProductId, summary.TotalQuantity, summary.TotalSalesAmount));
            }

            builder.AppendLine();
            builder.AppendLine("[週次集計]");
            builder.AppendLine("週\t販売数量\t売上金額");

            foreach (var summary in snapshot.WeeklySummaries) {
                builder.AppendLine(string.Format(
                    "{0:yyyy/MM/dd}-{1:yyyy/MM/dd}\t{2}\t{3}",
                    summary.WeekStartDate,
                    summary.WeekEndDate,
                    summary.TotalQuantity,
                    summary.TotalSalesAmount));
            }

            return builder.ToString();
        }
    }
}
