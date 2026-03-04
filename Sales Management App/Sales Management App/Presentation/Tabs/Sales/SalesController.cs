using System;
using System.Linq;
using SalesManagementApp.Core.Application.Exceptions;
using SalesManagementApp.Core.Application.Services;
using SalesManagementApp.Core.Application.State;
using SalesManagementApp.Core.Domain.Entities;
using Sales_Management_App.Presentation.Common;

namespace Sales_Management_App.Presentation.Tabs.Sales {
    /// <summary>
    /// 売上登録 タブのイベント処理を担当し、画面とサービスを接続します。
    /// </summary>
    internal sealed class SalesController {
        private readonly SalesView FView;
        private readonly ProductService FProductService;
        private readonly SalesService FSalesService;
        private readonly AppDataRepository FRepository;
        private readonly AppState FAppState;
        private readonly UiMessageService FMessageService;
        private readonly UiActionExecutor FActionExecutor;
        private readonly Action FOnSalesRegistered;

        internal SalesController(
            SalesView view,
            ProductService productService,
            SalesService salesService,
            AppDataRepository repository,
            AppState appState,
            UiMessageService messageService,
            UiActionExecutor actionExecutor,
            Action onSalesRegistered) {
            FView = view ?? throw new ArgumentNullException("view");
            FProductService = productService ?? throw new ArgumentNullException("productService");
            FSalesService = salesService ?? throw new ArgumentNullException("salesService");
            FRepository = repository ?? throw new ArgumentNullException("repository");
            FAppState = appState ?? throw new ArgumentNullException("appState");
            FMessageService = messageService ?? throw new ArgumentNullException("messageService");
            FActionExecutor = actionExecutor ?? throw new ArgumentNullException("actionExecutor");
            FOnSalesRegistered = onSalesRegistered ?? delegate { };
        }

        internal void Initialize() {
            FView.RegisterRequested += OnRegisterRequested;
            FView.ClearInputRequested += OnClearInputRequested;
            FView.FilterRequested += OnFilterRequested;
            FView.FilterClearRequested += OnFilterClearRequested;
            FView.InputChanged += OnInputChanged;
        }

        internal void RefreshProductOptions() {
            var selectedId = FView.GetSelectedProductOption()?.ProductId ?? string.Empty;
            var options = FProductService.GetAll(FAppState.Products).Select(p => new SaleProductOption {
                ProductId = p.ProductId,
                ProductName = p.ProductName,
                UnitPrice = p.UnitPrice
            }).ToList();
            FView.SetProductOptions(options, selectedId);
        }

        internal void RefreshGrid() {
            var filter = FView.GetFilter();
            var productNameMap = FAppState.Products.ToDictionary(p => p.ProductId, p => p.ProductName);
            var filtered = FSalesService.GetFiltered(
                FAppState.Sales,
                filter.StartDate,
                filter.EndDate,
                filter.StoreId,
                filter.ProductId);

            var rows = filtered.Select(s => new SalesViewRow {
                SaleDate = s.SaleDate.ToString("yyyy/MM/dd"),
                StoreId = s.StoreId,
                ProductId = s.ProductId,
                ProductName = ResolveProductName(productNameMap, s.ProductId),
                Quantity = s.Quantity,
                SalesAmount = s.SalesAmount
            }).ToList();
            FView.SetRows(rows);
        }

        internal void UpdatePricePreview() {
            var selected = FView.GetSelectedProductOption();
            if (selected == null) {
                FView.SetUnitPriceLabel("-");
                FView.SetAmountPreviewLabel("-");
                return;
            }

            FView.SetUnitPriceLabel(string.Format("{0} 円", selected.UnitPrice));

            var input = FView.GetInput();
            int quantity;
            if (!int.TryParse(input.QuantityText, out quantity) || quantity <= 0) {
                FView.SetAmountPreviewLabel("-");
                return;
            }

            FView.SetAmountPreviewLabel(string.Format("{0} 円", selected.UnitPrice * quantity));
        }

        private void OnRegisterRequested(object sender, EventArgs e) {
            FActionExecutor.Execute(delegate {
                var input = CreateSaleRecordFromInput(FView.GetInput());
                var registered = FSalesService.RegisterSale(
                    FAppState.Sales,
                    FAppState.Products,
                    FAppState.Inventories,
                    input,
                    FAppState.InventoryHistories,
                    DateTime.Now);
                FRepository.WriteInventoryHistories(FAppState.InventoryHistoryPath, FAppState.InventoryHistories);

                RefreshGrid();
                FView.ClearInput();
                UpdatePricePreview();
                FOnSalesRegistered.Invoke();
                FMessageService.ShowInfo(string.Format("売上を登録しました。金額: {0} 円", registered.SalesAmount));
            });
        }

        private void OnClearInputRequested(object sender, EventArgs e) {
            FView.ClearInput();
            UpdatePricePreview();
        }

        private void OnFilterRequested(object sender, EventArgs e) {
            FActionExecutor.Execute(delegate {
                RefreshGrid();
            });
        }

        private void OnFilterClearRequested(object sender, EventArgs e) {
            FView.ClearFilter();
            RefreshGrid();
        }

        private void OnInputChanged(object sender, EventArgs e) {
            UpdatePricePreview();
        }

        private static SaleRecord CreateSaleRecordFromInput(SalesInputModel input) {
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

        private static string ResolveProductName(
            System.Collections.Generic.IReadOnlyDictionary<string, string> productNameMap,
            string productId) {
            string productName;
            if (productNameMap.TryGetValue(productId, out productName)) {
                return productName;
            }

            return "(未登録商品)";
        }
    }
}
