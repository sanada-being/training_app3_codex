using System;
using System.Collections.Generic;
using System.Windows.Forms;
using SalesManagementApp.Core.Application.Services;
using SalesManagementApp.Core.Application.State;
using SalesManagementApp.Core.Domain.Entities;
using Sales_Management_App.Presentation.Common;
using Sales_Management_App.Presentation.Tabs.Inventory;
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

        private List<Product> _products { get { return _appState.Products; } }

        private List<InventoryRecord> _inventories { get { return _appState.Inventories; } }

        private List<InventoryHistoryRecord> _inventoryHistories { get { return _appState.InventoryHistories; } }

        private List<SaleRecord> _sales { get { return _appState.Sales; } }

        private string _inventoryHistoryPath { get { return _appState.InventoryHistoryPath; } }

        private ProductsView _productsView;
        private ProductsController _productsController;
        private InventoryView _inventoryView;
        private InventoryController _inventoryController;
        private SalesView _salesView;
        private SalesController _salesController;

        private DataGridView _inventoryHistoryGrid;
        private DateTimePicker _historyStartDatePicker;
        private DateTimePicker _historyEndDatePicker;
        private ComboBox _historyOperationTypeCombo;
        private TextBox _historyStoreIdText;
        private TextBox _historyProductIdText;

        private DateTimePicker _summaryStartDatePicker;
        private DateTimePicker _summaryEndDatePicker;
        private Label _summaryTotalLabel;
        private DataGridView _productSummaryGrid;
        private DataGridView _weeklySummaryGrid;
        private TextBox _aggregationFilterProductIdText;
        private AggregationSnapshot _currentAggregationSnapshot;

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
        }

        private void InitializeViewsFromState() {
            _productsController.Refresh();
            _inventoryController.Refresh();
            RefreshInventoryHistoryGrid();
            _salesController.RefreshProductOptions();
            _salesController.RefreshGrid();
            ResetAggregationDisplay();
            _salesController.UpdatePricePreview();
        }

        private void HandleProductsChanged() {
            _salesController.RefreshProductOptions();
            _salesController.RefreshGrid();
            _salesController.UpdatePricePreview();
        }

        private void HandleInventoryUpdated() {
            RefreshInventoryHistoryGrid();
        }

        private void HandleSalesRegistered() {
            _inventoryController.Refresh();
            RefreshInventoryHistoryGrid();
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

    }
}

