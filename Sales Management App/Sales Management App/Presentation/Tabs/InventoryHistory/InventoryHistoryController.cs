using System;
using System.Linq;
using SalesManagementApp.Core.Application.Services;
using SalesManagementApp.Core.Application.State;
using SalesManagementApp.Core.Domain.Entities;
using Sales_Management_App.Presentation.Common;

namespace Sales_Management_App.Presentation.Tabs.InventoryHistory {
    /// <summary>
    /// InventoryHistoryController クラスです。
    /// </summary>
    internal sealed class InventoryHistoryController {
        private readonly InventoryHistoryView FView;
        private readonly InventoryHistoryService FInventoryHistoryService;
        private readonly AppState FAppState;
        private readonly UiActionExecutor FActionExecutor;

        internal InventoryHistoryController(
            InventoryHistoryView view,
            InventoryHistoryService inventoryHistoryService,
            AppState appState,
            UiActionExecutor actionExecutor) {
            FView = view ?? throw new ArgumentNullException("view");
            FInventoryHistoryService = inventoryHistoryService ?? throw new ArgumentNullException("inventoryHistoryService");
            FAppState = appState ?? throw new ArgumentNullException("appState");
            FActionExecutor = actionExecutor ?? throw new ArgumentNullException("actionExecutor");
        }

        internal void Initialize() {
            FView.FilterRequested += OnFilterRequested;
            FView.FilterClearRequested += OnFilterClearRequested;
        }

        internal void Refresh() {
            var filter = FView.GetFilter();
            var filtered = FInventoryHistoryService.Filter(
                FAppState.InventoryHistories,
                filter.StartDateTime,
                filter.EndDateTime,
                filter.StoreId,
                filter.ProductId,
                filter.OperationType);

            var rows = filtered.Select(h => new InventoryHistoryViewRow {
                OccurredAt = h.OccurredAt.ToString("yyyy/MM/dd HH:mm:ss"),
                OperationType = ToOperationTypeLabel(h.OperationType),
                StoreId = h.StoreId,
                ProductId = h.ProductId,
                Quantity = h.Quantity,
                ResultStock = h.ResultStock,
                Result = h.Result
            }).ToList();
            FView.SetRows(rows);
        }

        private void OnFilterRequested(object sender, EventArgs e) {
            FActionExecutor.Execute(delegate {
                Refresh();
            });
        }

        private void OnFilterClearRequested(object sender, EventArgs e) {
            FView.ClearFilter();
            Refresh();
        }

        private static string ToOperationTypeLabel(InventoryOperationType operationType) {
            switch (operationType) {
                case InventoryOperationType.Inbound:
                    return "入荷";
                case InventoryOperationType.Outbound:
                    return "出庫";
                case InventoryOperationType.Sale:
                    return "売上連動";
                default:
                    return operationType.ToString();
            }
        }
    }
}
