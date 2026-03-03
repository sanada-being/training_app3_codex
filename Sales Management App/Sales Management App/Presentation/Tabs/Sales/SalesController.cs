using System;
using System.Linq;
using SalesManagementApp.Core.Application.Exceptions;
using SalesManagementApp.Core.Application.Services;
using SalesManagementApp.Core.Application.State;
using SalesManagementApp.Core.Domain.Entities;
using Sales_Management_App.Presentation.Common;

namespace Sales_Management_App.Presentation.Tabs.Sales {
    internal sealed class SalesController {
        private readonly SalesView _view;
        private readonly ProductService _productService;
        private readonly SalesService _salesService;
        private readonly AppDataRepository _repository;
        private readonly AppState _appState;
        private readonly UiMessageService _messageService;
        private readonly UiActionExecutor _actionExecutor;
        private readonly Action _onSalesRegistered;

        internal SalesController(
            SalesView view,
            ProductService productService,
            SalesService salesService,
            AppDataRepository repository,
            AppState appState,
            UiMessageService messageService,
            UiActionExecutor actionExecutor,
            Action onSalesRegistered) {
            _view = view ?? throw new ArgumentNullException("view");
            _productService = productService ?? throw new ArgumentNullException("productService");
            _salesService = salesService ?? throw new ArgumentNullException("salesService");
            _repository = repository ?? throw new ArgumentNullException("repository");
            _appState = appState ?? throw new ArgumentNullException("appState");
            _messageService = messageService ?? throw new ArgumentNullException("messageService");
            _actionExecutor = actionExecutor ?? throw new ArgumentNullException("actionExecutor");
            _onSalesRegistered = onSalesRegistered ?? delegate { };
        }

        internal void Initialize() {
            _view.RegisterRequested += OnRegisterRequested;
            _view.ClearInputRequested += OnClearInputRequested;
            _view.FilterRequested += OnFilterRequested;
            _view.FilterClearRequested += OnFilterClearRequested;
            _view.InputChanged += OnInputChanged;
        }

        internal void RefreshProductOptions() {
            var selected = _view.GetSelectedProductOption();
            var selectedId = selected == null ? string.Empty : selected.ProductId;
            var options = _productService.GetAll(_appState.Products).Select(p => new SaleProductOption {
                ProductId = p.ProductId,
                ProductName = p.ProductName,
                UnitPrice = p.UnitPrice
            }).ToList();
            _view.SetProductOptions(options, selectedId);
        }

        internal void RefreshGrid() {
            var filter = _view.GetFilter();
            var productNameMap = _appState.Products.ToDictionary(p => p.ProductId, p => p.ProductName);
            var filtered = _salesService.GetFiltered(
                _appState.Sales,
                filter.StartDate,
                filter.EndDate,
                filter.StoreId,
                filter.ProductId);

            var rows = filtered.Select(s => new SalesViewRow {
                SaleDate = s.SaleDate.ToString("yyyy/MM/dd"),
                StoreId = s.StoreId,
                ProductId = s.ProductId,
                ProductName = productNameMap.ContainsKey(s.ProductId) ? productNameMap[s.ProductId] : "(未登録商品)",
                Quantity = s.Quantity,
                SalesAmount = s.SalesAmount
            }).ToList();
            _view.SetRows(rows);
        }

        internal void UpdatePricePreview() {
            var selected = _view.GetSelectedProductOption();
            if (selected == null) {
                _view.SetUnitPriceLabel("-");
                _view.SetAmountPreviewLabel("-");
                return;
            }

            _view.SetUnitPriceLabel(string.Format("{0} 円", selected.UnitPrice));

            var input = _view.GetInput();
            int quantity;
            if (!int.TryParse(input.QuantityText, out quantity) || quantity <= 0) {
                _view.SetAmountPreviewLabel("-");
                return;
            }

            _view.SetAmountPreviewLabel(string.Format("{0} 円", selected.UnitPrice * quantity));
        }

        private void OnRegisterRequested(object sender, EventArgs e) {
            _actionExecutor.Execute(delegate {
                var input = BuildSaleInput(_view.GetInput());
                var registered = _salesService.RegisterSale(
                    _appState.Sales,
                    _appState.Products,
                    _appState.Inventories,
                    input,
                    _appState.InventoryHistories,
                    DateTime.Now);
                _repository.WriteInventoryHistories(_appState.InventoryHistoryPath, _appState.InventoryHistories);

                RefreshGrid();
                _view.ClearInput();
                UpdatePricePreview();
                _onSalesRegistered.Invoke();
                _messageService.ShowInfo(string.Format("売上を登録しました。金額: {0} 円", registered.SalesAmount));
            });
        }

        private void OnClearInputRequested(object sender, EventArgs e) {
            _view.ClearInput();
            UpdatePricePreview();
        }

        private void OnFilterRequested(object sender, EventArgs e) {
            _actionExecutor.Execute(delegate {
                RefreshGrid();
            });
        }

        private void OnFilterClearRequested(object sender, EventArgs e) {
            _view.ClearFilter();
            RefreshGrid();
        }

        private void OnInputChanged(object sender, EventArgs e) {
            UpdatePricePreview();
        }

        private static SaleRecord BuildSaleInput(SalesInputModel input) {
            if (string.IsNullOrWhiteSpace(input.ProductId)) {
                throw new DomainValidationException("商品を選択してください。");
            }

            int quantity;
            if (!int.TryParse(input.QuantityText, out quantity)) {
                throw new DomainValidationException("数量は整数で入力してください。");
            }

            return new SaleRecord {
                SaleDate = input.SaleDate,
                StoreId = input.StoreId,
                ProductId = input.ProductId,
                Quantity = quantity
            };
        }
    }
}
