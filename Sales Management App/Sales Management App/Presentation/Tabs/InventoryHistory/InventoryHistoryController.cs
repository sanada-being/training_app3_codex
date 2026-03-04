using System;
using System.Linq;
using SalesManagementApp.Core.Application.Services;
using SalesManagementApp.Core.Application.State;
using SalesManagementApp.Core.Domain.Entities;
using Sales_Management_App.Presentation.Common;

namespace Sales_Management_App.Presentation.Tabs.InventoryHistory {
    /// <summary>
    /// 在庫履歴 タブのイベント処理を担当し、画面とサービスを接続します。
    /// </summary>
    internal sealed class InventoryHistoryController {
        private readonly InventoryHistoryView FView;
        private readonly InventoryHistoryService FInventoryHistoryService;
        private readonly AppState FAppState;
        private readonly UiActionExecutor FActionExecutor;

        internal InventoryHistoryController(
            InventoryHistoryView vView,
            InventoryHistoryService vInventoryHistoryService,
            AppState vAppState,
            UiActionExecutor vActionExecutor) {
            FView = vView ?? throw new ArgumentNullException("view");
            FInventoryHistoryService = vInventoryHistoryService ?? throw new ArgumentNullException("inventoryHistoryService");
            FAppState = vAppState ?? throw new ArgumentNullException("appState");
            FActionExecutor = vActionExecutor ?? throw new ArgumentNullException("actionExecutor");
        }

        internal void Initialize() {
            FView.FilterRequested += OnFilterRequested;
            FView.FilterClearRequested += OnFilterClearRequested;
        }

        internal void Refresh() {
            var wFilter = FView.GetFilter();
            var wFiltered = FInventoryHistoryService.Filter(
                FAppState.InventoryHistories,
                wFilter.StartDateTime,
                wFilter.EndDateTime,
                wFilter.StoreId,
                wFilter.ProductId,
                wFilter.OperationType);

            var wRows = wFiltered.Select(vH => new InventoryHistoryViewRow {
                OccurredAt = vH.OccurredAt.ToString("yyyy/MM/dd HH:mm:ss"),
                OperationType = ToOperationTypeLabel(vH.OperationType),
                StoreId = vH.StoreId,
                ProductId = vH.ProductId,
                Quantity = vH.Quantity,
                ResultStock = vH.ResultStock,
                Result = vH.Result
            }).ToList();
            FView.SetRows(wRows);
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

        private static string ToOperationTypeLabel(InventoryOperationTypeEnum vOperationType) {
            switch (vOperationType) {
                case InventoryOperationTypeEnum.Inbound:
                    return "入荷";
                case InventoryOperationTypeEnum.Outbound:
                    return "出庫";
                case InventoryOperationTypeEnum.Sale:
                    return "売上連動";
                default:
                    return vOperationType.ToString();
            }
        }
    }
}

