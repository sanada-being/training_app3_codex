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
            ProductsView vView,
            ProductService vProductService,
            AppState vAppState,
            UiMessageService vMessageService,
            UiActionExecutor vActionExecutor,
            Action vOnProductsChanged) {
            FView = vView ?? throw new ArgumentNullException("view");
            FProductService = vProductService ?? throw new ArgumentNullException("productService");
            FAppState = vAppState ?? throw new ArgumentNullException("appState");
            FMessageService = vMessageService ?? throw new ArgumentNullException("messageService");
            FActionExecutor = vActionExecutor ?? throw new ArgumentNullException("actionExecutor");
            FOnProductsChanged = vOnProductsChanged ?? delegate { };
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
            var wFilter = FView.GetFilter();
            var wFiltered = FProductService.GetFiltered(FAppState.Products, wFilter.ProductId, wFilter.ProductName, wFilter.Category);
            var wRows = wFiltered.Select(vP => new ProductViewRow {
                ProductId = vP.ProductId,
                ProductName = vP.ProductName,
                UnitPrice = vP.UnitPrice,
                Category = vP.Category
            }).ToList();
            FView.SetRows(wRows);
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
            var wProductId = FView.GetDeleteTargetProductId();
            if (string.IsNullOrWhiteSpace(wProductId)) {
                FMessageService.ShowWarning("削除対象の商品IDを選択してください。", "入力エラー");
                return;
            }

            if (FMessageService.Confirm(string.Format("商品ID={0} を削除します。よろしいですか？", wProductId), "確認") != DialogResult.Yes) {
                return;
            }

            FActionExecutor.Execute(delegate {
                FProductService.Delete(FAppState.Products, wProductId);
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
            var wSelected = FView.GetSelectedRow();
            if (wSelected == null) {
                return;
            }

            FView.SetInput(new ProductInputModel {
                ProductId = wSelected.ProductId,
                ProductName = wSelected.ProductName,
                UnitPriceText = wSelected.UnitPrice.ToString(),
                Category = wSelected.Category
            });
        }

        private static Product CreateProductFromInput(ProductInputModel vInput) {
            int wUnitPrice;
            if (!int.TryParse(vInput.UnitPriceText, out wUnitPrice)) {
                throw new DomainValidationException("単価は整数で入力してください。");
            }

            return new Product {
                ProductId = vInput.ProductId,
                ProductName = vInput.ProductName,
                UnitPrice = wUnitPrice,
                Category = vInput.Category
            };
        }
    }
}
