using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SalesManagementApp.Core.Application.Services;
using SalesManagementApp.Core.Application.State;
using Sales_Management_App.Presentation.Common;

namespace Sales_Management_App.Presentation.Tabs.Aggregation {
    /// <summary>
    /// AggregationController クラスです。
    /// </summary>
    internal sealed class AggregationController {
        private readonly AggregationView FView;
        private readonly SalesAggregationService FSalesAggregationService;
        private readonly AppState FAppState;
        private readonly UiMessageService FMessageService;
        private readonly UiActionExecutor FActionExecutor;

        private AggregationSnapshot FCurrentSnapshot;

        internal AggregationController(
            AggregationView view,
            SalesAggregationService salesAggregationService,
            AppState appState,
            UiMessageService messageService,
            UiActionExecutor actionExecutor) {
            FView = view ?? throw new ArgumentNullException("view");
            FSalesAggregationService = salesAggregationService ?? throw new ArgumentNullException("salesAggregationService");
            FAppState = appState ?? throw new ArgumentNullException("appState");
            FMessageService = messageService ?? throw new ArgumentNullException("messageService");
            FActionExecutor = actionExecutor ?? throw new ArgumentNullException("actionExecutor");
        }

        internal void Initialize() {
            FView.ExecuteRequested += OnExecuteRequested;
            FView.CopyRequested += OnCopyRequested;
            FView.FilterRequested += OnFilterRequested;
            FView.FilterClearRequested += OnFilterClearRequested;
        }

        internal void Reset() {
            FCurrentSnapshot = null;
            FView.ResetDisplay();
        }

        private void OnExecuteRequested(object sender, EventArgs e) {
            FActionExecutor.Execute(delegate {
                var snapshot = CreateAggregationSnapshot();
                RenderAggregationSnapshot(snapshot);
            });
        }

        private void OnCopyRequested(object sender, EventArgs e) {
            FActionExecutor.Execute(delegate {
                var snapshot = CreateAggregationSnapshot();
                RenderAggregationSnapshot(snapshot);
                Clipboard.SetText(CreateAggregationClipboardText(snapshot));
                FMessageService.ShowInfo("集計結果をクリップボードにコピーしました。");
            });
        }

        private void OnFilterRequested(object sender, EventArgs e) {
            ApplyFilter();
        }

        private void OnFilterClearRequested(object sender, EventArgs e) {
            FView.ClearProductIdFilter();
            ApplyFilter();
        }

        private AggregationSnapshot CreateAggregationSnapshot() {
            var startDate = FView.GetStartDate();
            var endDate = FView.GetEndDate();

            return new AggregationSnapshot {
                StartDate = startDate,
                EndDate = endDate,
                ProductSummaries = FSalesAggregationService.GetProductSummaries(FAppState.Sales, startDate, endDate).ToList(),
                WeeklySummaries = FSalesAggregationService.GetWeeklySummaries(FAppState.Sales, startDate, endDate).ToList(),
                TotalSalesAmount = FSalesAggregationService.GetTotalSalesAmount(FAppState.Sales, startDate, endDate)
            };
        }

        private void RenderAggregationSnapshot(AggregationSnapshot snapshot) {
            FCurrentSnapshot = snapshot;
            FView.SetSummaryText(string.Format(
                "期間: {0:yyyy/MM/dd} - {1:yyyy/MM/dd} / 合計売上: {2} 円",
                snapshot.StartDate,
                snapshot.EndDate,
                snapshot.TotalSalesAmount));
            ApplyFilter();
        }

        private void ApplyFilter() {
            if (FCurrentSnapshot == null) {
                FView.SetProductRows(new List<AggregationProductSummaryRow>());
                FView.SetWeeklyRows(new List<AggregationWeeklySummaryRow>());
                return;
            }

            var filter = FView.GetProductIdFilter();
            var filteredProductSummaries = FCurrentSnapshot.ProductSummaries;
            if (!string.IsNullOrWhiteSpace(filter)) {
                filteredProductSummaries = filteredProductSummaries
                    .Where(s => s.ProductId.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0)
                    .ToList();
            }

            var productRows = filteredProductSummaries.Select(s => new AggregationProductSummaryRow {
                ProductId = s.ProductId,
                TotalQuantity = s.TotalQuantity,
                TotalSalesAmount = s.TotalSalesAmount
            }).ToList();
            FView.SetProductRows(productRows);

            var weeklyRows = FCurrentSnapshot.WeeklySummaries.Select(s => new AggregationWeeklySummaryRow {
                Week = string.Format("{0:yyyy/MM/dd} - {1:yyyy/MM/dd}", s.WeekStartDate, s.WeekEndDate),
                TotalQuantity = s.TotalQuantity,
                TotalSalesAmount = s.TotalSalesAmount
            }).ToList();
            FView.SetWeeklyRows(weeklyRows);
        }

        private static string CreateAggregationClipboardText(AggregationSnapshot snapshot) {
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
