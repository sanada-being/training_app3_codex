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
            SalesView vView,
            ProductService vProductService,
            SalesService vSalesService,
            AppDataRepository vRepository,
            AppState vAppState,
            UiMessageService vMessageService,
            UiActionExecutor vActionExecutor,
            Action vOnSalesRegistered) {
            FView = vView ?? throw new ArgumentNullException("view");
            FProductService = vProductService ?? throw new ArgumentNullException("productService");
            FSalesService = vSalesService ?? throw new ArgumentNullException("salesService");
            FRepository = vRepository ?? throw new ArgumentNullException("repository");
            FAppState = vAppState ?? throw new ArgumentNullException("appState");
            FMessageService = vMessageService ?? throw new ArgumentNullException("messageService");
            FActionExecutor = vActionExecutor ?? throw new ArgumentNullException("actionExecutor");
            FOnSalesRegistered = vOnSalesRegistered ?? delegate { };
        }

        internal void Initialize() {
            FView.RegisterRequested += OnRegisterRequested;
            FView.ClearInputRequested += OnClearInputRequested;
            FView.FilterRequested += OnFilterRequested;
            FView.FilterClearRequested += OnFilterClearRequested;
            FView.InputChanged += OnInputChanged;
        }

        internal void RefreshProductOptions() {
            var wSelectedId = FView.GetSelectedProductOption()?.ProductId ?? string.Empty;
            var wOptions = FProductService.GetAll(FAppState.Products).Select(vP => new SaleProductOption {
                ProductId = vP.ProductId,
                ProductName = vP.ProductName,
                UnitPrice = vP.UnitPrice
            }).ToList();
            FView.SetProductOptions(wOptions, wSelectedId);
        }

        internal void RefreshGrid() {
            var wFilter = FView.GetFilter();
            var wProductNameMap = FAppState.Products.ToDictionary(vP => vP.ProductId, vP => vP.ProductName);
            var wFiltered = FSalesService.GetFiltered(
                FAppState.Sales,
                wFilter.StartDate,
                wFilter.EndDate,
                wFilter.StoreId,
                wFilter.ProductId);

            var wRows = wFiltered.Select(vS => new SalesViewRow {
                SaleDate = vS.SaleDate.ToString("yyyy/MM/dd"),
                StoreId = vS.StoreId,
                ProductId = vS.ProductId,
                ProductName = ResolveProductName(wProductNameMap, vS.ProductId),
                Quantity = vS.Quantity,
                SalesAmount = vS.SalesAmount
            }).ToList();
            FView.SetRows(wRows);
        }

        internal void UpdatePricePreview() {
            var wSelected = FView.GetSelectedProductOption();
            if (wSelected == null) {
                FView.SetUnitPriceLabel("-");
                FView.SetAmountPreviewLabel("-");
                return;
            }

            FView.SetUnitPriceLabel(string.Format("{0} 円", wSelected.UnitPrice));

            var wInput = FView.GetInput();
            int wQuantity;
            if (!int.TryParse(wInput.QuantityText, out wQuantity) || wQuantity <= 0) {
                FView.SetAmountPreviewLabel("-");
                return;
            }

            FView.SetAmountPreviewLabel(string.Format("{0} 円", wSelected.UnitPrice * wQuantity));
        }

        private void OnRegisterRequested(object sender, EventArgs e) {
            FActionExecutor.Execute(delegate {
                var wInput = CreateSaleRecordFromInput(FView.GetInput());
                var wRegistered = FSalesService.RegisterSale(
                    FAppState.Sales,
                    FAppState.Products,
                    FAppState.Inventories,
                    wInput,
                    FAppState.InventoryHistories,
                    DateTime.Now);
                FRepository.WriteInventoryHistories(FAppState.InventoryHistoryPath, FAppState.InventoryHistories);

                RefreshGrid();
                FView.ClearInput();
                UpdatePricePreview();
                FOnSalesRegistered.Invoke();
                FMessageService.ShowInfo(string.Format("売上を登録しました。金額: {0} 円", wRegistered.SalesAmount));
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

        private static SaleRecord CreateSaleRecordFromInput(SalesInputModel vInput) {
            if (string.IsNullOrWhiteSpace(vInput.ProductId)) {
                throw new DomainValidationException("商品を選択してください。");
            }

            int wQuantity;
            if (!int.TryParse(vInput.QuantityText, out wQuantity)) {
                throw new DomainValidationException("数量は整数で入力してください。");
            }

            return new SaleRecord {
                SaleDate = vInput.SaleDate,
                StoreId = vInput.StoreId,
                ProductId = vInput.ProductId,
                Quantity = wQuantity
            };
        }

        private static string ResolveProductName(
            System.Collections.Generic.IReadOnlyDictionary<string, string> vProductNameMap,
            string vProductId) {
            string wProductName;
            if (vProductNameMap.TryGetValue(vProductId, out wProductName)) {
                return wProductName;
            }

            return "(未登録商品)";
        }
    }
}
