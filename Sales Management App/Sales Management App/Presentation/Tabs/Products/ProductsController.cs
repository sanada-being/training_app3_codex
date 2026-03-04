using System;
using System.Linq;
using System.Windows.Forms;
using SalesManagementApp.Core.Application.Exceptions;
using SalesManagementApp.Core.Application.Services;
using SalesManagementApp.Core.Application.State;
using SalesManagementApp.Core.Domain.Entities;
using Sales_Management_App.Presentation.Common;

namespace Sales_Management_App.Presentation.Tabs.Products {
    internal sealed class ProductsController {
        private readonly ProductsView _view;
        private readonly ProductService _productService;
        private readonly AppState _appState;
        private readonly UiMessageService _messageService;
        private readonly UiActionExecutor _actionExecutor;
        private readonly Action _onProductsChanged;

        internal ProductsController(
            ProductsView view,
            ProductService productService,
            AppState appState,
            UiMessageService messageService,
            UiActionExecutor actionExecutor,
            Action onProductsChanged) {
            _view = view ?? throw new ArgumentNullException("view");
            _productService = productService ?? throw new ArgumentNullException("productService");
            _appState = appState ?? throw new ArgumentNullException("appState");
            _messageService = messageService ?? throw new ArgumentNullException("messageService");
            _actionExecutor = actionExecutor ?? throw new ArgumentNullException("actionExecutor");
            _onProductsChanged = onProductsChanged ?? delegate { };
        }

        internal void Initialize() {
            _view.RegisterRequested += OnRegisterRequested;
            _view.UpdateRequested += OnUpdateRequested;
            _view.DeleteRequested += OnDeleteRequested;
            _view.ClearInputRequested += OnClearInputRequested;
            _view.FilterRequested += OnFilterRequested;
            _view.FilterClearRequested += OnFilterClearRequested;
            _view.SelectedProductChanged += OnSelectedProductChanged;
        }

        internal void Refresh() {
            var filter = _view.GetFilter();
            var filtered = _productService.GetFiltered(_appState.Products, filter.ProductId, filter.ProductName, filter.Category);
            var rows = filtered.Select(p => new ProductViewRow {
                ProductId = p.ProductId,
                ProductName = p.ProductName,
                UnitPrice = p.UnitPrice,
                Category = p.Category
            }).ToList();
            _view.SetRows(rows);
        }

        private void OnRegisterRequested(object sender, EventArgs e) {
            _actionExecutor.Execute(delegate {
                _productService.Register(_appState.Products, CreateProductFromInput(_view.GetInput()));
                Refresh();
                _view.ClearInput();
                _onProductsChanged.Invoke();
            });
        }

        private void OnUpdateRequested(object sender, EventArgs e) {
            _actionExecutor.Execute(delegate {
                _productService.Update(_appState.Products, CreateProductFromInput(_view.GetInput()));
                Refresh();
                _onProductsChanged.Invoke();
            });
        }

        private void OnDeleteRequested(object sender, EventArgs e) {
            var productId = _view.GetDeleteTargetProductId();
            if (string.IsNullOrWhiteSpace(productId)) {
                _messageService.ShowWarning("削除対象の商品IDを選択してください。", "入力エラー");
                return;
            }

            if (_messageService.Confirm(string.Format("商品ID={0} を削除します。よろしいですか？", productId), "確認") != DialogResult.Yes) {
                return;
            }

            _actionExecutor.Execute(delegate {
                _productService.Delete(_appState.Products, productId);
                Refresh();
                _view.ClearInput();
                _onProductsChanged.Invoke();
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

        private void OnSelectedProductChanged(object sender, EventArgs e) {
            var selected = _view.GetSelectedRow();
            if (selected == null) {
                return;
            }

            _view.SetInput(new ProductInputModel {
                ProductId = selected.ProductId,
                ProductName = selected.ProductName,
                UnitPriceText = selected.UnitPrice.ToString(),
                Category = selected.Category
            });
        }

        private static Product CreateProductFromInput(ProductInputModel input) {
            int unitPrice;
            if (!int.TryParse(input.UnitPriceText, out unitPrice)) {
                throw new DomainValidationException("単価は整数で入力してください。");
            }

            return new Product {
                ProductId = input.ProductId,
                ProductName = input.ProductName,
                UnitPrice = unitPrice,
                Category = input.Category
            };
        }
    }
}
