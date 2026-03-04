using System;
using System.Windows.Forms;
using SalesManagementApp.Core.Application.Exceptions;
using SalesManagementApp.Core.Application.Services;
using SalesManagementApp.Core.Application.State;
using Sales_Management_App.Presentation.Common;
using Sales_Management_App.Presentation.Tabs.Aggregation;
using Sales_Management_App.Presentation.Tabs.Inventory;
using Sales_Management_App.Presentation.Tabs.InventoryHistory;
using Sales_Management_App.Presentation.Tabs.Products;
using Sales_Management_App.Presentation.Tabs.Sales;

namespace Sales_Management_App {
    /// <summary>
    /// メイン画面として各タブを初期化し画面間連携を構成します。
    /// </summary>
    public partial class Form1 : Form {
        private readonly ProductService FProductService;
        private readonly InventoryService FInventoryService;
        private readonly InventoryHistoryService FInventoryHistoryService;
        private readonly SalesService FSalesService;
        private readonly SalesAggregationService FSalesAggregationService;
        private readonly AppDataRepository FAppDataRepository;
        private readonly AppBootstrapper FAppBootstrapper;
        private readonly UiMessageService FMessageService;
        private readonly UiActionExecutor FUiActionExecutor;

        private readonly AppState FAppState = new AppState();

        private ProductsView FProductsView;
        private ProductsController FProductsController;
        private InventoryView FInventoryView;
        private InventoryController FInventoryController;
        private InventoryHistoryView FInventoryHistoryView;
        private InventoryHistoryController FInventoryHistoryController;
        private SalesView FSalesView;
        private SalesController FSalesController;
        private AggregationView FAggregationView;
        private AggregationController FAggregationController;

        /// <summary>
        /// 依存関係を既定設定で初期化してフォームを生成します。
        /// </summary>
        public Form1()
            : this(MainFormDependencies.CreateDefault()) {
        }

        internal Form1(MainFormDependencies vDependencies) {
            if (vDependencies == null) {
                throw new ArgumentNullException("dependencies");
            }

            FProductService = vDependencies.ProductService;
            FInventoryService = vDependencies.InventoryService;
            FInventoryHistoryService = vDependencies.InventoryHistoryService;
            FSalesService = vDependencies.SalesService;
            FSalesAggregationService = vDependencies.SalesAggregationService;
            FAppDataRepository = vDependencies.AppDataRepository;
            FAppBootstrapper = vDependencies.AppBootstrapper;
            FMessageService = new UiMessageService();
            FUiActionExecutor = new UiActionExecutor(FMessageService);

            InitializeComponent();
            InitializeShell();
            InitializeControllers();
            LoadInitialDataFromRepositoryRoot();
            InitializeViewsFromState();
        }

        private void InitializeControllers() {
            FProductsController = new ProductsController(
                FProductsView,
                FProductService,
                FAppState,
                FMessageService,
                FUiActionExecutor,
                HandleProductsChanged);
            FProductsController.Initialize();

            FInventoryController = new InventoryController(
                FInventoryView,
                FInventoryService,
                FAppDataRepository,
                FAppState,
                FMessageService,
                FUiActionExecutor,
                HandleInventoryUpdated);
            FInventoryController.Initialize();

            FInventoryHistoryController = new InventoryHistoryController(
                FInventoryHistoryView,
                FInventoryHistoryService,
                FAppState,
                FUiActionExecutor);
            FInventoryHistoryController.Initialize();

            FSalesController = new SalesController(
                FSalesView,
                FProductService,
                FSalesService,
                FAppDataRepository,
                FAppState,
                FMessageService,
                FUiActionExecutor,
                HandleSalesRegistered);
            FSalesController.Initialize();

            FAggregationController = new AggregationController(
                FAggregationView,
                FSalesAggregationService,
                FAppState,
                FMessageService,
                FUiActionExecutor);
            FAggregationController.Initialize();
        }

        private void InitializeViewsFromState() {
            FProductsController.Refresh();
            FInventoryController.Refresh();
            FInventoryHistoryController.Refresh();
            FSalesController.RefreshProductOptions();
            FSalesController.RefreshGrid();
            FAggregationController.Reset();
            FSalesController.UpdatePricePreview();
        }

        private void HandleProductsChanged() {
            FSalesController.RefreshProductOptions();
            FSalesController.RefreshGrid();
            FSalesController.UpdatePricePreview();
        }

        private void HandleInventoryUpdated() {
            FInventoryHistoryController.Refresh();
        }

        private void HandleSalesRegistered() {
            FInventoryController.Refresh();
            FInventoryHistoryController.Refresh();
        }

        private void InitializeShell() {
            Text = "Sales Management App";
            Width = 1100;
            Height = 700;

            var wTabs = new TabControl { Dock = DockStyle.Fill };
            wTabs.TabPages.Add(CreateProductTab());
            wTabs.TabPages.Add(CreateInventoryTab());
            wTabs.TabPages.Add(CreateInventoryHistoryTab());
            wTabs.TabPages.Add(CreateSalesTab());
            wTabs.TabPages.Add(CreateAggregationTab());
            Controls.Add(wTabs);
        }

        private TabPage CreateProductTab() {
            var wTab = new TabPage("商品管理");
            FProductsView = new ProductsView();
            wTab.Controls.Add(FProductsView);
            return wTab;
        }

        private TabPage CreateInventoryTab() {
            var wTab = new TabPage("在庫管理");
            FInventoryView = new InventoryView();
            wTab.Controls.Add(FInventoryView);
            return wTab;
        }

        private TabPage CreateSalesTab() {
            var wTab = new TabPage("売上登録");
            FSalesView = new SalesView();
            wTab.Controls.Add(FSalesView);
            return wTab;
        }

        private TabPage CreateInventoryHistoryTab() {
            var wTab = new TabPage("在庫履歴");
            FInventoryHistoryView = new InventoryHistoryView();
            wTab.Controls.Add(FInventoryHistoryView);
            return wTab;
        }

        private TabPage CreateAggregationTab() {
            var wTab = new TabPage("売上集計");
            FAggregationView = new AggregationView();
            wTab.Controls.Add(FAggregationView);
            return wTab;
        }

        private void LoadInitialDataFromRepositoryRoot() {
            try {
                var wLoaded = FAppBootstrapper.LoadFromBaseDirectory(AppDomain.CurrentDomain.BaseDirectory);
                FAppState.Products = wLoaded.Products;
                FAppState.Inventories = wLoaded.Inventories;
                FAppState.InventoryHistories = wLoaded.InventoryHistories;
                FAppState.Sales = wLoaded.Sales;
                FAppState.RepositoryRootPath = wLoaded.RepositoryRootPath;
                FAppState.ProductsPath = wLoaded.ProductsPath;
                FAppState.InventoryPath = wLoaded.InventoryPath;
                FAppState.SalesPath = wLoaded.SalesPath;
                FAppState.InventoryHistoryPath = wLoaded.InventoryHistoryPath;
            } catch (DomainValidationException wEx) {
                FMessageService.ShowWarning(string.Format("初期データの読み込みに失敗しました: {0}", wEx.Message), "入力エラー");
            } catch (Exception wEx) {
                FMessageService.ShowError(wEx);
            }
        }

    }
}

