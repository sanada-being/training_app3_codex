using System;
using System.Linq;
using SalesManagementApp.Core.Application.Services;
using SalesManagementApp.Core.Application.State;
using SalesManagementApp.Core.Domain.Entities;
using Sales_Management_App.Presentation.Common;

namespace Sales_Management_App.Presentation.Tabs.InventoryHistory {
    internal sealed class InventoryHistoryController {
        private readonly InventoryHistoryView _view;
        private readonly InventoryHistoryService _inventoryHistoryService;
        private readonly AppState _appState;
        private readonly UiActionExecutor _actionExecutor;

        internal InventoryHistoryController(
            InventoryHistoryView view,
            InventoryHistoryService inventoryHistoryService,
            AppState appState,
            UiActionExecutor actionExecutor) {
            _view = view ?? throw new ArgumentNullException("view");
            _inventoryHistoryService = inventoryHistoryService ?? throw new ArgumentNullException("inventoryHistoryService");
            _appState = appState ?? throw new ArgumentNullException("appState");
            _actionExecutor = actionExecutor ?? throw new ArgumentNullException("actionExecutor");
        }

        internal void Initialize() {
            _view.FilterRequested += OnFilterRequested;
            _view.FilterClearRequested += OnFilterClearRequested;
        }

        internal void Refresh() {
            var filter = _view.GetFilter();
            var filtered = _inventoryHistoryService.Filter(
                _appState.InventoryHistories,
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
            _view.SetRows(rows);
        }

        private void OnFilterRequested(object sender, EventArgs e) {
            _actionExecutor.Execute(delegate {
                Refresh();
            });
        }

        private void OnFilterClearRequested(object sender, EventArgs e) {
            _view.ClearFilter();
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
