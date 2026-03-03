using System;
using System.Linq;
using SalesManagementApp.Core.Application.Exceptions;
using SalesManagementApp.Core.Application.Services;
using SalesManagementApp.Core.Application.State;
using SalesManagementApp.Core.Domain.Entities;
using Sales_Management_App.Presentation.Common;

namespace Sales_Management_App.Presentation.Tabs.Inventory {
    internal sealed class InventoryController {
        private readonly InventoryView _view;
        private readonly InventoryService _inventoryService;
        private readonly AppDataRepository _repository;
        private readonly AppState _appState;
        private readonly UiMessageService _messageService;
        private readonly UiActionExecutor _actionExecutor;
        private readonly Action _onInventoryUpdated;

        internal InventoryController(
            InventoryView view,
            InventoryService inventoryService,
            AppDataRepository repository,
            AppState appState,
            UiMessageService messageService,
            UiActionExecutor actionExecutor,
            Action onInventoryUpdated) {
            _view = view ?? throw new ArgumentNullException("view");
            _inventoryService = inventoryService ?? throw new ArgumentNullException("inventoryService");
            _repository = repository ?? throw new ArgumentNullException("repository");
            _appState = appState ?? throw new ArgumentNullException("appState");
            _messageService = messageService ?? throw new ArgumentNullException("messageService");
            _actionExecutor = actionExecutor ?? throw new ArgumentNullException("actionExecutor");
            _onInventoryUpdated = onInventoryUpdated ?? delegate { };
        }

        internal void Initialize() {
            _view.AddRequested += OnAddRequested;
            _view.RemoveRequested += OnRemoveRequested;
            _view.ClearInputRequested += OnClearInputRequested;
            _view.FilterRequested += OnFilterRequested;
            _view.FilterClearRequested += OnFilterClearRequested;
            _view.SelectedInventoryChanged += OnSelectedInventoryChanged;
        }

        internal void Refresh() {
            var filter = _view.GetFilter();
            var filtered = _inventoryService.GetFiltered(_appState.Inventories, filter.StoreId, filter.ProductId);
            var rows = filtered.Select(i => new InventoryViewRow {
                StoreId = i.StoreId,
                ProductId = i.ProductId,
                Stock = i.Stock
            }).ToList();
            _view.SetRows(rows);
            _view.SetReorderCount(_inventoryService.GetReorderTargets(_appState.Inventories, 5).Count);
        }

        private void OnAddRequested(object sender, EventArgs e) {
            _actionExecutor.Execute(delegate {
                var input = BuildInput(_view.GetInput());
                _inventoryService.AddStock(
                    _appState.Inventories,
                    input.StoreId,
                    input.ProductId,
                    input.Stock,
                    _appState.InventoryHistories,
                    DateTime.Now);
                _repository.WriteInventoryHistories(_appState.InventoryHistoryPath, _appState.InventoryHistories);

                Refresh();
                _view.ClearInput();
                _onInventoryUpdated.Invoke();
                _messageService.ShowInfo("入荷を反映しました。");
            });
        }

        private void OnRemoveRequested(object sender, EventArgs e) {
            _actionExecutor.Execute(delegate {
                var input = BuildInput(_view.GetInput());
                _inventoryService.RemoveStock(
                    _appState.Inventories,
                    input.StoreId,
                    input.ProductId,
                    input.Stock,
                    _appState.InventoryHistories,
                    DateTime.Now);
                _repository.WriteInventoryHistories(_appState.InventoryHistoryPath, _appState.InventoryHistories);

                Refresh();
                _view.ClearInput();
                _onInventoryUpdated.Invoke();
                _messageService.ShowInfo("出庫を反映しました。");
            });
        }

        private void OnClearInputRequested(object sender, EventArgs e) {
            _view.ClearInput();
        }

        private void OnFilterRequested(object sender, EventArgs e) {
            Refresh();
        }

        private void OnFilterClearRequested(object sender, EventArgs e) {
            _view.ClearFilter();
            Refresh();
        }

        private void OnSelectedInventoryChanged(object sender, EventArgs e) {
            var selected = _view.GetSelectedRow();
            if (selected == null) {
                return;
            }

            _view.SetInput(new InventoryInputModel {
                StoreId = selected.StoreId,
                ProductId = selected.ProductId,
                QuantityText = string.Empty
            });
        }

        private static InventoryRecord BuildInput(InventoryInputModel input) {
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
