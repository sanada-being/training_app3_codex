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
    /// 販売管理アプリケーションのメインフォームです。
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

        internal Form1(MainFormDependencies dependencies) {
            if (dependencies == null) {
                throw new ArgumentNullException("dependencies");
            }

            FProductService = dependencies.ProductService;
            FInventoryService = dependencies.InventoryService;
            FInventoryHistoryService = dependencies.InventoryHistoryService;
            FSalesService = dependencies.SalesService;
            FSalesAggregationService = dependencies.SalesAggregationService;
            FAppDataRepository = dependencies.AppDataRepository;
            FAppBootstrapper = dependencies.AppBootstrapper;
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

            var tabs = new TabControl { Dock = DockStyle.Fill };
            tabs.TabPages.Add(CreateProductTab());
            tabs.TabPages.Add(CreateInventoryTab());
            tabs.TabPages.Add(CreateInventoryHistoryTab());
            tabs.TabPages.Add(CreateSalesTab());
            tabs.TabPages.Add(CreateAggregationTab());
            Controls.Add(tabs);
        }

        private TabPage CreateProductTab() {
            var tab = new TabPage("商品管理");
            FProductsView = new ProductsView();
            tab.Controls.Add(FProductsView);
            return tab;
        }

        private TabPage CreateInventoryTab() {
            var tab = new TabPage("在庫管理");
            FInventoryView = new InventoryView();
            tab.Controls.Add(FInventoryView);
            return tab;
        }

        private TabPage CreateSalesTab() {
            var tab = new TabPage("売上登録");
            FSalesView = new SalesView();
            tab.Controls.Add(FSalesView);
            return tab;
        }

        private TabPage CreateInventoryHistoryTab() {
            var tab = new TabPage("在庫履歴");
            FInventoryHistoryView = new InventoryHistoryView();
            tab.Controls.Add(FInventoryHistoryView);
            return tab;
        }

        private TabPage CreateAggregationTab() {
            var tab = new TabPage("売上集計");
            FAggregationView = new AggregationView();
            tab.Controls.Add(FAggregationView);
            return tab;
        }

        private void LoadInitialDataFromRepositoryRoot() {
            try {
                var loaded = FAppBootstrapper.LoadFromBaseDirectory(AppDomain.CurrentDomain.BaseDirectory);
                FAppState.Products = loaded.Products;
                FAppState.Inventories = loaded.Inventories;
                FAppState.InventoryHistories = loaded.InventoryHistories;
                FAppState.Sales = loaded.Sales;
                FAppState.RepositoryRootPath = loaded.RepositoryRootPath;
                FAppState.ProductsPath = loaded.ProductsPath;
                FAppState.InventoryPath = loaded.InventoryPath;
                FAppState.SalesPath = loaded.SalesPath;
                FAppState.InventoryHistoryPath = loaded.InventoryHistoryPath;
            } catch (DomainValidationException ex) {
                FMessageService.ShowWarning(string.Format("初期データの読み込みに失敗しました: {0}", ex.Message), "入力エラー");
            } catch (Exception ex) {
                FMessageService.ShowError(ex);
            }
        }

    }
}

