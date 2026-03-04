using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SalesManagementApp.Core.Application.Services;
using SalesManagementApp.Core.Application.State;
using Sales_Management_App.Presentation.Common;

namespace Sales_Management_App.Presentation.Tabs.Aggregation {
    internal sealed class AggregationController {
        private readonly AggregationView _view;
        private readonly SalesAggregationService _salesAggregationService;
        private readonly AppState _appState;
        private readonly UiMessageService _messageService;
        private readonly UiActionExecutor _actionExecutor;

        private AggregationSnapshot _currentSnapshot;

        internal AggregationController(
            AggregationView view,
            SalesAggregationService salesAggregationService,
            AppState appState,
            UiMessageService messageService,
            UiActionExecutor actionExecutor) {
            _view = view ?? throw new ArgumentNullException("view");
            _salesAggregationService = salesAggregationService ?? throw new ArgumentNullException("salesAggregationService");
            _appState = appState ?? throw new ArgumentNullException("appState");
            _messageService = messageService ?? throw new ArgumentNullException("messageService");
            _actionExecutor = actionExecutor ?? throw new ArgumentNullException("actionExecutor");
        }

        internal void Initialize() {
            _view.ExecuteRequested += OnExecuteRequested;
            _view.CopyRequested += OnCopyRequested;
            _view.FilterRequested += OnFilterRequested;
            _view.FilterClearRequested += OnFilterClearRequested;
        }

        internal void Reset() {
            _currentSnapshot = null;
            _view.ResetDisplay();
        }

        private void OnExecuteRequested(object sender, EventArgs e) {
            _actionExecutor.Execute(delegate {
                var snapshot = CreateAggregationSnapshot();
                RenderAggregationSnapshot(snapshot);
            });
        }

        private void OnCopyRequested(object sender, EventArgs e) {
            _actionExecutor.Execute(delegate {
                var snapshot = CreateAggregationSnapshot();
                RenderAggregationSnapshot(snapshot);
                Clipboard.SetText(CreateAggregationClipboardText(snapshot));
                _messageService.ShowInfo("集計結果をクリップボードにコピーしました。");
            });
        }

        private void OnFilterRequested(object sender, EventArgs e) {
            ApplyFilter();
        }

        private void OnFilterClearRequested(object sender, EventArgs e) {
            _view.ClearProductIdFilter();
            ApplyFilter();
        }

        private AggregationSnapshot CreateAggregationSnapshot() {
            var startDate = _view.GetStartDate();
            var endDate = _view.GetEndDate();

            return new AggregationSnapshot {
                StartDate = startDate,
                EndDate = endDate,
                ProductSummaries = _salesAggregationService.GetProductSummaries(_appState.Sales, startDate, endDate).ToList(),
                WeeklySummaries = _salesAggregationService.GetWeeklySummaries(_appState.Sales, startDate, endDate).ToList(),
                TotalSalesAmount = _salesAggregationService.GetTotalSalesAmount(_appState.Sales, startDate, endDate)
            };
        }

        private void RenderAggregationSnapshot(AggregationSnapshot snapshot) {
            _currentSnapshot = snapshot;
            _view.SetSummaryText(string.Format(
                "期間: {0:yyyy/MM/dd} - {1:yyyy/MM/dd} / 合計売上: {2} 円",
                snapshot.StartDate,
                snapshot.EndDate,
                snapshot.TotalSalesAmount));
            ApplyFilter();
        }

        private void ApplyFilter() {
            if (_currentSnapshot == null) {
                _view.SetProductRows(new List<AggregationProductSummaryRow>());
                _view.SetWeeklyRows(new List<AggregationWeeklySummaryRow>());
                return;
            }

            var filter = _view.GetProductIdFilter();
            var filteredProductSummaries = _currentSnapshot.ProductSummaries;
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
            _view.SetProductRows(productRows);

            var weeklyRows = _currentSnapshot.WeeklySummaries.Select(s => new AggregationWeeklySummaryRow {
                Week = string.Format("{0:yyyy/MM/dd} - {1:yyyy/MM/dd}", s.WeekStartDate, s.WeekEndDate),
                TotalQuantity = s.TotalQuantity,
                TotalSalesAmount = s.TotalSalesAmount
            }).ToList();
            _view.SetWeeklyRows(weeklyRows);
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
