using System;
using System.Linq;
using System.Windows.Forms;
using SalesManagementApp.Core.Application.Exceptions;
using SalesManagementApp.Core.Application.Services;
using SalesManagementApp.Core.Application.State;
using SalesManagementApp.Core.Domain.Entities;
using Sales_Management_App.Presentation.Common;

namespace Sales_Management_App.Presentation.Tabs.Products {
    /// <summary>
    /// 商品管理 タブのイベント処理を担当し、画面とサービスを接続します。
    /// </summary>
    internal sealed class ProductsController {
        private readonly ProductsView FView;
        private readonly ProductService FProductService;
        private readonly AppState FAppState;
        private readonly UiMessageService FMessageService;
        private readonly UiActionExecutor FActionExecutor;
        private readonly Action FOnProductsChanged;

        internal ProductsController(
            ProductsView view,
            ProductService productService,
            AppState appState,
            UiMessageService messageService,
            UiActionExecutor actionExecutor,
            Action onProductsChanged) {
            FView = view ?? throw new ArgumentNullException("view");
            FProductService = productService ?? throw new ArgumentNullException("productService");
            FAppState = appState ?? throw new ArgumentNullException("appState");
            FMessageService = messageService ?? throw new ArgumentNullException("messageService");
            FActionExecutor = actionExecutor ?? throw new ArgumentNullException("actionExecutor");
            FOnProductsChanged = onProductsChanged ?? delegate { };
        }

        internal void Initialize() {
            FView.RegisterRequested += OnRegisterRequested;
            FView.UpdateRequested += OnUpdateRequested;
            FView.DeleteRequested += OnDeleteRequested;
            FView.ClearInputRequested += OnClearInputRequested;
            FView.FilterRequested += OnFilterRequested;
            FView.FilterClearRequested += OnFilterClearRequested;
            FView.SelectedProductChanged += OnSelectedProductChanged;
        }

        internal void Refresh() {
            var filter = FView.GetFilter();
            var filtered = FProductService.GetFiltered(FAppState.Products, filter.ProductId, filter.ProductName, filter.Category);
            var rows = filtered.Select(p => new ProductViewRow {
                ProductId = p.ProductId,
                ProductName = p.ProductName,
                UnitPrice = p.UnitPrice,
                Category = p.Category
            }).ToList();
            FView.SetRows(rows);
        }

        private void OnRegisterRequested(object sender, EventArgs e) {
            FActionExecutor.Execute(delegate {
                FProductService.Register(FAppState.Products, CreateProductFromInput(FView.GetInput()));
                Refresh();
                FView.ClearInput();
                FOnProductsChanged.Invoke();
            });
        }

        private void OnUpdateRequested(object sender, EventArgs e) {
            FActionExecutor.Execute(delegate {
                FProductService.Update(FAppState.Products, CreateProductFromInput(FView.GetInput()));
                Refresh();
                FOnProductsChanged.Invoke();
            });
        }

        private void OnDeleteRequested(object sender, EventArgs e) {
            var productId = FView.GetDeleteTargetProductId();
            if (string.IsNullOrWhiteSpace(productId)) {
                FMessageService.ShowWarning("削除対象の商品IDを選択してください。", "入力エラー");
                return;
            }

            if (FMessageService.Confirm(string.Format("商品ID={0} を削除します。よろしいですか？", productId), "確認") != DialogResult.Yes) {
                return;
            }

            FActionExecutor.Execute(delegate {
                FProductService.Delete(FAppState.Products, productId);
                Refresh();
                FView.ClearInput();
                FOnProductsChanged.Invoke();
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

        private void OnSelectedProductChanged(object sender, EventArgs e) {
            var selected = FView.GetSelectedRow();
            if (selected == null) {
                return;
            }

            FView.SetInput(new ProductInputModel {
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
