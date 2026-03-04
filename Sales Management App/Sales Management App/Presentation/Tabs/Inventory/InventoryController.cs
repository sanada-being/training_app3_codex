using System;
using System.Linq;
using SalesManagementApp.Core.Application.Exceptions;
using SalesManagementApp.Core.Application.Services;
using SalesManagementApp.Core.Application.State;
using SalesManagementApp.Core.Domain.Entities;
using Sales_Management_App.Presentation.Common;

namespace Sales_Management_App.Presentation.Tabs.Inventory {
    /// <summary>
    /// 在庫管理 タブのイベント処理を担当し、画面とサービスを接続します。
    /// </summary>
    internal sealed class InventoryController {
        private readonly InventoryView FView;
        private readonly InventoryService FInventoryService;
        private readonly AppDataRepository FRepository;
        private readonly AppState FAppState;
        private readonly UiMessageService FMessageService;
        private readonly UiActionExecutor FActionExecutor;
        private readonly Action FOnInventoryUpdated;

        internal InventoryController(
            InventoryView vView,
            InventoryService vInventoryService,
            AppDataRepository vRepository,
            AppState vAppState,
            UiMessageService vMessageService,
            UiActionExecutor vActionExecutor,
            Action vOnInventoryUpdated) {
            FView = vView ?? throw new ArgumentNullException("view");
            FInventoryService = vInventoryService ?? throw new ArgumentNullException("inventoryService");
            FRepository = vRepository ?? throw new ArgumentNullException("repository");
            FAppState = vAppState ?? throw new ArgumentNullException("appState");
            FMessageService = vMessageService ?? throw new ArgumentNullException("messageService");
            FActionExecutor = vActionExecutor ?? throw new ArgumentNullException("actionExecutor");
            FOnInventoryUpdated = vOnInventoryUpdated ?? delegate { };
        }

        internal void Initialize() {
            FView.AddRequested += OnAddRequested;
            FView.RemoveRequested += OnRemoveRequested;
            FView.ClearInputRequested += OnClearInputRequested;
            FView.FilterRequested += OnFilterRequested;
            FView.FilterClearRequested += OnFilterClearRequested;
            FView.SelectedInventoryChanged += OnSelectedInventoryChanged;
        }

        internal void Refresh() {
            var wFilter = FView.GetFilter();
            var wFiltered = FInventoryService.GetFiltered(FAppState.Inventories, wFilter.StoreId, wFilter.ProductId);
            var wRows = wFiltered.Select(vI => new InventoryViewRow {
                StoreId = vI.StoreId,
                ProductId = vI.ProductId,
                Stock = vI.Stock
            }).ToList();
            FView.SetRows(wRows);
            FView.SetReorderCount(FInventoryService.GetReorderTargets(FAppState.Inventories, 5).Count);
        }

        private void OnAddRequested(object sender, EventArgs e) {
            FActionExecutor.Execute(delegate {
                var wInput = CreateInventoryRecordFromInput(FView.GetInput());
                FInventoryService.AddStock(
                    FAppState.Inventories,
                    wInput.StoreId,
                    wInput.ProductId,
                    wInput.Stock,
                    FAppState.InventoryHistories,
                    DateTime.Now);
                FRepository.WriteInventoryHistories(FAppState.InventoryHistoryPath, FAppState.InventoryHistories);

                Refresh();
                FView.ClearInput();
                FOnInventoryUpdated.Invoke();
                FMessageService.ShowInfo("入荷を反映しました。");
            });
        }

        private void OnRemoveRequested(object sender, EventArgs e) {
            FActionExecutor.Execute(delegate {
                var wInput = CreateInventoryRecordFromInput(FView.GetInput());
                FInventoryService.RemoveStock(
                    FAppState.Inventories,
                    wInput.StoreId,
                    wInput.ProductId,
                    wInput.Stock,
                    FAppState.InventoryHistories,
                    DateTime.Now);
                FRepository.WriteInventoryHistories(FAppState.InventoryHistoryPath, FAppState.InventoryHistories);

                Refresh();
                FView.ClearInput();
                FOnInventoryUpdated.Invoke();
                FMessageService.ShowInfo("出庫を反映しました。");
            });
        }

        private void OnClearInputRequested(object sender, EventArgs e) {
            FView.ClearInput();
        }

        private void OnFilterRequested(object sender, EventArgs e) {
            Refresh();
        }

        private void OnFilterClearRequested(object sender, EventArgs e) {
            FView.ClearFilter();
            Refresh();
        }

        private void OnSelectedInventoryChanged(object sender, EventArgs e) {
            var wSelected = FView.GetSelectedRow();
            if (wSelected == null) {
                return;
            }

            FView.SetInput(new InventoryInputModel {
                StoreId = wSelected.StoreId,
                ProductId = wSelected.ProductId,
                QuantityText = string.Empty
            });
        }

        private static InventoryRecord CreateInventoryRecordFromInput(InventoryInputModel vInput) {
            int wQuantity;
            if (!int.TryParse(vInput.QuantityText, out wQuantity)) {
                throw new DomainValidationException("数量は整数で入力してください。");
            }

            return new InventoryRecord {
                StoreId = vInput.StoreId,
                ProductId = vInput.ProductId,
                Stock = wQuantity
            };
        }
    }
}
