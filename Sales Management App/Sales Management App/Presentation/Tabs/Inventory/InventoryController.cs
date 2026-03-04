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
            InventoryView view,
            InventoryService inventoryService,
            AppDataRepository repository,
            AppState appState,
            UiMessageService messageService,
            UiActionExecutor actionExecutor,
            Action onInventoryUpdated) {
            FView = view ?? throw new ArgumentNullException("view");
            FInventoryService = inventoryService ?? throw new ArgumentNullException("inventoryService");
            FRepository = repository ?? throw new ArgumentNullException("repository");
            FAppState = appState ?? throw new ArgumentNullException("appState");
            FMessageService = messageService ?? throw new ArgumentNullException("messageService");
            FActionExecutor = actionExecutor ?? throw new ArgumentNullException("actionExecutor");
            FOnInventoryUpdated = onInventoryUpdated ?? delegate { };
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
            var filter = FView.GetFilter();
            var filtered = FInventoryService.GetFiltered(FAppState.Inventories, filter.StoreId, filter.ProductId);
            var rows = filtered.Select(i => new InventoryViewRow {
                StoreId = i.StoreId,
                ProductId = i.ProductId,
                Stock = i.Stock
            }).ToList();
            FView.SetRows(rows);
            FView.SetReorderCount(FInventoryService.GetReorderTargets(FAppState.Inventories, 5).Count);
        }

        private void OnAddRequested(object sender, EventArgs e) {
            FActionExecutor.Execute(delegate {
                var input = CreateInventoryRecordFromInput(FView.GetInput());
                FInventoryService.AddStock(
                    FAppState.Inventories,
                    input.StoreId,
                    input.ProductId,
                    input.Stock,
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
                var input = CreateInventoryRecordFromInput(FView.GetInput());
                FInventoryService.RemoveStock(
                    FAppState.Inventories,
                    input.StoreId,
                    input.ProductId,
                    input.Stock,
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
            var selected = FView.GetSelectedRow();
            if (selected == null) {
                return;
            }

            FView.SetInput(new InventoryInputModel {
                StoreId = selected.StoreId,
                ProductId = selected.ProductId,
                QuantityText = string.Empty
            });
        }

        private static InventoryRecord CreateInventoryRecordFromInput(InventoryInputModel input) {
            int quantity;
            if (!int.TryParse(input.QuantityText, out quantity)) {
                throw new DomainValidationException("数量は整数で入力してください。");
            }

            return new InventoryRecord {
                StoreId = input.StoreId,
                ProductId = input.ProductId,
                Stock = quantity
            };
        }
    }
}
