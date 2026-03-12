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
    /// 売上集計 タブのイベント処理を担当し、画面とサービスを接続します。
    /// </summary>
    internal sealed class AggregationController {
        private readonly AggregationView FView;
        private readonly SalesAggregationService FSalesAggregationService;
        private readonly AppState FAppState;
        private readonly UiMessageService FMessageService;
        private readonly UiActionExecutor FActionExecutor;

        private AggregationSnapshot FCurrentSnapshot;

        internal AggregationController(
            AggregationView vView,
            SalesAggregationService vSalesAggregationService,
            AppState vAppState,
            UiMessageService vMessageService,
            UiActionExecutor vActionExecutor) {
            FView = vView ?? throw new ArgumentNullException("view");
            FSalesAggregationService = vSalesAggregationService ?? throw new ArgumentNullException("salesAggregationService");
            FAppState = vAppState ?? throw new ArgumentNullException("appState");
            FMessageService = vMessageService ?? throw new ArgumentNullException("messageService");
            FActionExecutor = vActionExecutor ?? throw new ArgumentNullException("actionExecutor");
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
                var wSnapshot = CreateAggregationSnapshot();
                RenderAggregationSnapshot(wSnapshot);
            });
        }

        private void OnCopyRequested(object sender, EventArgs e) {
            FActionExecutor.Execute(delegate {
                var wSnapshot = CreateAggregationSnapshot();
                RenderAggregationSnapshot(wSnapshot);
                Clipboard.SetText(CreateAggregationClipboardText(wSnapshot));
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
            var wStartDate = FView.GetStartDate();
            var wEndDate = FView.GetEndDate();

            return new AggregationSnapshot {
                StartDate = wStartDate,
                EndDate = wEndDate,
                ProductSummaries = FSalesAggregationService.GetProductSummaries(FAppState.Sales, wStartDate, wEndDate).ToList(),
                WeeklySummaries = FSalesAggregationService.GetWeeklySummaries(FAppState.Sales, wStartDate, wEndDate).ToList(),
                TotalSalesAmount = FSalesAggregationService.GetTotalSalesAmount(FAppState.Sales, wStartDate, wEndDate)
            };
        }

        private void RenderAggregationSnapshot(AggregationSnapshot vSnapshot) {
            FCurrentSnapshot = vSnapshot;
            FView.SetSummaryText(string.Format(
                "期間: {0:yyyy/MM/dd} - {1:yyyy/MM/dd} / 合計売上: {2} 円",
                vSnapshot.StartDate,
                vSnapshot.EndDate,
                vSnapshot.TotalSalesAmount));
            ApplyFilter();
        }

        private void ApplyFilter() {
            if (FCurrentSnapshot == null) {
                FView.SetProductRows(new List<AggregationProductSummaryRow>());
                FView.SetWeeklyRows(new List<AggregationWeeklySummaryRow>());
                return;
            }

            var wFilter = FView.GetProductIdFilter();
            var wFilteredProductSummaries = FCurrentSnapshot.ProductSummaries;
            if (!string.IsNullOrWhiteSpace(wFilter)) {
                wFilteredProductSummaries = wFilteredProductSummaries
                    .Where(vS => vS.ProductId.IndexOf(wFilter, StringComparison.OrdinalIgnoreCase) >= 0)
                    .ToList();
            }

            var wProductRows = wFilteredProductSummaries.Select(vS => new AggregationProductSummaryRow {
                ProductId = vS.ProductId,
                TotalQuantity = vS.TotalQuantity,
                TotalSalesAmount = vS.TotalSalesAmount
            }).ToList();
            FView.SetProductRows(wProductRows);

            var wWeeklyRows = FCurrentSnapshot.WeeklySummaries.Select(vS => new AggregationWeeklySummaryRow {
                Week = string.Format("{0:yyyy/MM/dd} - {1:yyyy/MM/dd}", vS.WeekStartDate, vS.WeekEndDate),
                TotalQuantity = vS.TotalQuantity,
                TotalSalesAmount = vS.TotalSalesAmount
            }).ToList();
            FView.SetWeeklyRows(wWeeklyRows);
        }

        private static string CreateAggregationClipboardText(AggregationSnapshot vSnapshot) {
            var wBuilder = new StringBuilder();
            wBuilder.AppendLine(string.Format("期間: {0:yyyy/MM/dd} - {1:yyyy/MM/dd}", vSnapshot.StartDate, vSnapshot.EndDate));
            wBuilder.AppendLine(string.Format("合計売上: {0} 円", vSnapshot.TotalSalesAmount));
            wBuilder.AppendLine();
            wBuilder.AppendLine("[商品別集計]");
            wBuilder.AppendLine("商品ID\t販売数量\t売上金額");

            foreach (var wSummary in vSnapshot.ProductSummaries) {
                wBuilder.AppendLine(string.Format("{0}\t{1}\t{2}", wSummary.ProductId, wSummary.TotalQuantity, wSummary.TotalSalesAmount));
            }

            wBuilder.AppendLine();
            wBuilder.AppendLine("[週次集計]");
            wBuilder.AppendLine("週\t販売数量\t売上金額");

            foreach (var wSummary in vSnapshot.WeeklySummaries) {
                wBuilder.AppendLine(string.Format(
                    "{0:yyyy/MM/dd}-{1:yyyy/MM/dd}\t{2}\t{3}",
                    wSummary.WeekStartDate,
                    wSummary.WeekEndDate,
                    wSummary.TotalQuantity,
                    wSummary.TotalSalesAmount));
            }

            return wBuilder.ToString();
        }
    }
}
