using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SalesManagementApp.Core.Application.Exceptions;
using SalesManagementApp.Core.Application.Models;
using SalesManagementApp.Core.Application.Services;
using SalesManagementApp.Core.Domain.Entities;
using SalesManagementApp.Core.Infrastructure.Csv;

namespace Sales_Management_App {
    public partial class Form1 : Form {
        private readonly ProductService _productService = new ProductService();
        private readonly InventoryService _inventoryService = new InventoryService();
        private readonly SalesService _salesService = new SalesService();
        private readonly SalesAggregationService _salesAggregationService = new SalesAggregationService();
        private readonly CsvDataStore _csvDataStore = new CsvDataStore();

        private readonly List<Product> _products = new List<Product>();
        private readonly List<InventoryRecord> _inventories = new List<InventoryRecord>();
        private readonly List<SaleRecord> _sales = new List<SaleRecord>();

        private DataGridView _productsGrid;
        private TextBox _productIdText;
        private TextBox _productNameText;
        private TextBox _unitPriceText;
        private TextBox _categoryText;

        private DataGridView _inventoryGrid;
        private TextBox _inventoryStoreIdText;
        private TextBox _inventoryProductIdText;
        private TextBox _inventoryQuantityText;
        private Label _reorderLabel;

        private DataGridView _salesGrid;
        private DateTimePicker _saleDatePicker;
        private TextBox _saleStoreIdText;
        private ComboBox _saleProductCombo;
        private TextBox _saleQuantityText;
        private Label _saleUnitPriceLabel;
        private Label _saleAmountPreviewLabel;

        private DateTimePicker _summaryStartDatePicker;
        private DateTimePicker _summaryEndDatePicker;
        private Label _summaryTotalLabel;
        private DataGridView _productSummaryGrid;
        private DataGridView _weeklySummaryGrid;

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
            tabs.TabPages.Add(CreateSalesTab());
            tabs.TabPages.Add(CreateAggregationTab());
            Controls.Add(tabs);

            WireSalesInputValidation();
            RefreshProductsGrid();
            RefreshInventoryGrid();
            RefreshSaleProductOptions();
            RefreshSalesGrid();
            ResetAggregationDisplay();
            UpdateSaleUnitPriceAndAmountPreview();
        }

    }
}

