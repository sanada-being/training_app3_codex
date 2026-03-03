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
    public partial class Form1 : Form {
        private readonly ProductService _productService;
        private readonly InventoryService _inventoryService;
        private readonly InventoryHistoryService _inventoryHistoryService;
        private readonly SalesService _salesService;
        private readonly SalesAggregationService _salesAggregationService;
        private readonly AppDataRepository _appDataRepository;
        private readonly AppBootstrapper _appBootstrapper;
        private readonly UiMessageService _messageService;
        private readonly UiActionExecutor _uiActionExecutor;

        private readonly AppState _appState = new AppState();

        private ProductsView _productsView;
        private ProductsController _productsController;
        private InventoryView _inventoryView;
        private InventoryController _inventoryController;
        private InventoryHistoryView _inventoryHistoryView;
        private InventoryHistoryController _inventoryHistoryController;
        private SalesView _salesView;
        private SalesController _salesController;
        private AggregationView _aggregationView;
        private AggregationController _aggregationController;

        public Form1()
            : this(MainFormDependencies.CreateDefault()) {
        }

        internal Form1(MainFormDependencies dependencies) {
            if (dependencies == null) {
                throw new ArgumentNullException("dependencies");
            }

            _productService = dependencies.ProductService;
            _inventoryService = dependencies.InventoryService;
            _inventoryHistoryService = dependencies.InventoryHistoryService;
            _salesService = dependencies.SalesService;
            _salesAggregationService = dependencies.SalesAggregationService;
            _appDataRepository = dependencies.AppDataRepository;
            _appBootstrapper = dependencies.AppBootstrapper;
            _messageService = new UiMessageService();
            _uiActionExecutor = new UiActionExecutor(_messageService);

            InitializeComponent();
            InitializeShell();
            InitializeControllers();
            LoadInitialDataFromRepositoryRoot();
            InitializeViewsFromState();
        }

        private void InitializeControllers() {
            _productsController = new ProductsController(
                _productsView,
                _productService,
                _appState,
                _messageService,
                _uiActionExecutor,
                HandleProductsChanged);
            _productsController.Initialize();

            _inventoryController = new InventoryController(
                _inventoryView,
                _inventoryService,
                _appDataRepository,
                _appState,
                _messageService,
                _uiActionExecutor,
                HandleInventoryUpdated);
            _inventoryController.Initialize();

            _inventoryHistoryController = new InventoryHistoryController(
                _inventoryHistoryView,
                _inventoryHistoryService,
                _appState,
                _uiActionExecutor);
            _inventoryHistoryController.Initialize();

            _salesController = new SalesController(
                _salesView,
                _productService,
                _salesService,
                _appDataRepository,
                _appState,
                _messageService,
                _uiActionExecutor,
                HandleSalesRegistered);
            _salesController.Initialize();

            _aggregationController = new AggregationController(
                _aggregationView,
                _salesAggregationService,
                _appState,
                _messageService,
                _uiActionExecutor);
            _aggregationController.Initialize();
        }

        private void InitializeViewsFromState() {
            _productsController.Refresh();
            _inventoryController.Refresh();
            _inventoryHistoryController.Refresh();
            _salesController.RefreshProductOptions();
            _salesController.RefreshGrid();
            _aggregationController.Reset();
            _salesController.UpdatePricePreview();
        }

        private void HandleProductsChanged() {
            _salesController.RefreshProductOptions();
            _salesController.RefreshGrid();
            _salesController.UpdatePricePreview();
        }

        private void HandleInventoryUpdated() {
            _inventoryHistoryController.Refresh();
        }

        private void HandleSalesRegistered() {
            _inventoryController.Refresh();
            _inventoryHistoryController.Refresh();
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
            _productsView = new ProductsView();
            tab.Controls.Add(_productsView);
            return tab;
        }

        private TabPage CreateInventoryTab() {
            var tab = new TabPage("在庫管理");
            _inventoryView = new InventoryView();
            tab.Controls.Add(_inventoryView);
            return tab;
        }

        private TabPage CreateSalesTab() {
            var tab = new TabPage("売上登録");
            _salesView = new SalesView();
            tab.Controls.Add(_salesView);
            return tab;
        }

        private TabPage CreateInventoryHistoryTab() {
            var tab = new TabPage("在庫履歴");
            _inventoryHistoryView = new InventoryHistoryView();
            tab.Controls.Add(_inventoryHistoryView);
            return tab;
        }

        private TabPage CreateAggregationTab() {
            var tab = new TabPage("売上集計");
            _aggregationView = new AggregationView();
            tab.Controls.Add(_aggregationView);
            return tab;
        }

        private void LoadInitialDataFromRepositoryRoot() {
            try {
                var loaded = _appBootstrapper.LoadFromBaseDirectory(AppDomain.CurrentDomain.BaseDirectory);
                _appState.Products = loaded.Products;
                _appState.Inventories = loaded.Inventories;
                _appState.InventoryHistories = loaded.InventoryHistories;
                _appState.Sales = loaded.Sales;
                _appState.RepositoryRootPath = loaded.RepositoryRootPath;
                _appState.ProductsPath = loaded.ProductsPath;
                _appState.InventoryPath = loaded.InventoryPath;
                _appState.SalesPath = loaded.SalesPath;
                _appState.InventoryHistoryPath = loaded.InventoryHistoryPath;
            } catch (DomainValidationException ex) {
                _messageService.ShowWarning(string.Format("初期データの読み込みに失敗しました: {0}", ex.Message), "入力エラー");
            } catch (Exception ex) {
                _messageService.ShowError(ex);
            }
        }

    }
}

