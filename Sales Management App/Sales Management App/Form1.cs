using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SalesManagementApp.Core.Application.Exceptions;
using SalesManagementApp.Core.Application.Models;
using SalesManagementApp.Core.Application.Services;
using SalesManagementApp.Core.Application.State;
using SalesManagementApp.Core.Domain.Entities;

namespace Sales_Management_App {
    public partial class Form1 : Form {
        private readonly ProductService _productService = new ProductService();
        private readonly InventoryService _inventoryService = new InventoryService();
        private readonly InventoryHistoryService _inventoryHistoryService = new InventoryHistoryService();
        private readonly SalesService _salesService = new SalesService();
        private readonly SalesAggregationService _salesAggregationService = new SalesAggregationService();
        private readonly AppDataRepository _appDataRepository = new AppDataRepository();
        private readonly AppBootstrapper _appBootstrapper = new AppBootstrapper();

        private AppState _appState = new AppState();

        private List<Product> _products { get { return _appState.Products; } }

        private List<InventoryRecord> _inventories { get { return _appState.Inventories; } }

        private List<InventoryHistoryRecord> _inventoryHistories { get { return _appState.InventoryHistories; } }

        private List<SaleRecord> _sales { get { return _appState.Sales; } }

        private string _inventoryHistoryPath { get { return _appState.InventoryHistoryPath; } }

        private DataGridView _productsGrid;
        private TextBox _productIdText;
        private TextBox _productNameText;
        private TextBox _unitPriceText;
        private TextBox _categoryText;
        private TextBox _productFilterIdText;
        private TextBox _productFilterNameText;
        private TextBox _productFilterCategoryText;

        private DataGridView _inventoryGrid;
        private TextBox _inventoryStoreIdText;
        private TextBox _inventoryProductIdText;
        private TextBox _inventoryQuantityText;
        private TextBox _inventoryFilterStoreIdText;
        private TextBox _inventoryFilterProductIdText;
        private Label _reorderLabel;
        private DataGridView _inventoryHistoryGrid;
        private DateTimePicker _historyStartDatePicker;
        private DateTimePicker _historyEndDatePicker;
        private ComboBox _historyOperationTypeCombo;
        private TextBox _historyStoreIdText;
        private TextBox _historyProductIdText;

        private DataGridView _salesGrid;
        private DateTimePicker _saleDatePicker;
        private TextBox _saleStoreIdText;
        private ComboBox _saleProductCombo;
        private TextBox _saleQuantityText;
        private DateTimePicker _salesFilterStartDatePicker;
        private DateTimePicker _salesFilterEndDatePicker;
        private TextBox _salesFilterStoreIdText;
        private TextBox _salesFilterProductIdText;
        private Label _saleUnitPriceLabel;
        private Label _saleAmountPreviewLabel;

        private DateTimePicker _summaryStartDatePicker;
        private DateTimePicker _summaryEndDatePicker;
        private Label _summaryTotalLabel;
        private DataGridView _productSummaryGrid;
        private DataGridView _weeklySummaryGrid;
        private TextBox _aggregationFilterProductIdText;
        private AggregationSnapshot _currentAggregationSnapshot;

        private ErrorProvider _errorProvider;

        public Form1() {
            InitializeComponent();
            InitializeMainTabs();
            LoadInitialDataFromRepositoryRoot();
        }

        private void InitializeMainTabs() {
            Text = "Sales Management App";
            Width = 1100;
            Height = 700;

            _errorProvider = new ErrorProvider {
                BlinkStyle = ErrorBlinkStyle.NeverBlink
            };
            _errorProvider.SetIconAlignment(this, ErrorIconAlignment.MiddleLeft);

            var tabs = new TabControl { Dock = DockStyle.Fill };
            tabs.TabPages.Add(CreateProductTab());
            tabs.TabPages.Add(CreateInventoryTab());
            tabs.TabPages.Add(CreateInventoryHistoryTab());
            tabs.TabPages.Add(CreateSalesTab());
            tabs.TabPages.Add(CreateAggregationTab());
            Controls.Add(tabs);

            WireSalesInputValidation();
            RefreshProductsGrid();
            RefreshInventoryGrid();
            RefreshInventoryHistoryGrid();
            RefreshSaleProductOptions();
            RefreshSalesGrid();
            ResetAggregationDisplay();
            UpdateSaleUnitPriceAndAmountPreview();
        }

    }
}

