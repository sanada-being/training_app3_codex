using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SalesManagementApp.Core.Application.Exceptions;
using SalesManagementApp.Core.Application.Models;
using SalesManagementApp.Core.Application.Services;
using SalesManagementApp.Core.Domain.Entities;

namespace Sales_Management_App {
    public partial class Form1 : Form {
        private readonly ProductService _productService = new ProductService();
        private readonly InventoryService _inventoryService = new InventoryService();
        private readonly SalesService _salesService = new SalesService();
        private readonly SalesAggregationService _salesAggregationService = new SalesAggregationService();

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

        private TabPage CreateProductTab() {
            var tab = new TabPage("商品管理");
            var root = new TableLayoutPanel {
                Dock = DockStyle.Fill,
                RowCount = 2,
                ColumnCount = 1
            };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 170));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            var inputPanel = new TableLayoutPanel {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 3,
                Padding = new Padding(12)
            };
            inputPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            inputPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            inputPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            inputPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

            _productIdText = AddLabeledTextBox(inputPanel, "商品ID", 0, 0);
            _productNameText = AddLabeledTextBox(inputPanel, "商品名", 2, 0);
            _unitPriceText = AddLabeledTextBox(inputPanel, "単価", 0, 1);
            _categoryText = AddLabeledTextBox(inputPanel, "区分", 2, 1);

            var buttonFlow = new FlowLayoutPanel {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = true
            };
            buttonFlow.Controls.Add(CreateButton("登録", RegisterProduct));
            buttonFlow.Controls.Add(CreateButton("更新", UpdateProduct));
            buttonFlow.Controls.Add(CreateButton("削除", DeleteProduct));
            buttonFlow.Controls.Add(CreateButton("クリア", delegate { ClearProductInputs(); }));
            inputPanel.Controls.Add(buttonFlow, 0, 2);
            inputPanel.SetColumnSpan(buttonFlow, 4);

            _productsGrid = new DataGridView {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AutoGenerateColumns = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false
            };
            _productsGrid.SelectionChanged += ProductsGridOnSelectionChanged;

            root.Controls.Add(inputPanel, 0, 0);
            root.Controls.Add(_productsGrid, 0, 1);
            tab.Controls.Add(root);
            return tab;
        }

        private TabPage CreateInventoryTab() {
            var tab = new TabPage("在庫管理");
            var root = new TableLayoutPanel {
                Dock = DockStyle.Fill,
                RowCount = 3,
                ColumnCount = 1
            };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 160));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            var inputPanel = new TableLayoutPanel {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 3,
                Padding = new Padding(12)
            };
            inputPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            inputPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            inputPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            inputPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

            _inventoryStoreIdText = AddLabeledTextBox(inputPanel, "店舗ID", 0, 0);
            _inventoryProductIdText = AddLabeledTextBox(inputPanel, "商品ID", 2, 0);
            _inventoryQuantityText = AddLabeledTextBox(inputPanel, "数量", 0, 1);

            var buttonFlow = new FlowLayoutPanel {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = true
            };
            buttonFlow.Controls.Add(CreateButton("入荷", AddInventory));
            buttonFlow.Controls.Add(CreateButton("出庫", RemoveInventory));
            buttonFlow.Controls.Add(CreateButton("クリア", delegate { ClearInventoryInputs(); }));
            inputPanel.Controls.Add(buttonFlow, 0, 2);
            inputPanel.SetColumnSpan(buttonFlow, 4);

            _reorderLabel = new Label {
                Dock = DockStyle.Fill,
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft,
                Padding = new Padding(12, 0, 0, 0)
            };

            _inventoryGrid = new DataGridView {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AutoGenerateColumns = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false
            };
            _inventoryGrid.SelectionChanged += InventoryGridOnSelectionChanged;

            root.Controls.Add(inputPanel, 0, 0);
            root.Controls.Add(_reorderLabel, 0, 1);
            root.Controls.Add(_inventoryGrid, 0, 2);
            tab.Controls.Add(root);
            return tab;
        }

        private TabPage CreateSalesTab() {
            var tab = new TabPage("売上登録");
            var root = new TableLayoutPanel {
                Dock = DockStyle.Fill,
                RowCount = 3,
                ColumnCount = 1
            };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 200));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            var inputPanel = new TableLayoutPanel {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 4,
                Padding = new Padding(12)
            };
            inputPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            inputPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            inputPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            inputPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

            var saleDateLabel = new Label {
                Text = "販売日",
                Dock = DockStyle.Fill,
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft
            };
            _saleDatePicker = new DateTimePicker {
                Dock = DockStyle.Fill,
                Format = DateTimePickerFormat.Short
            };
            inputPanel.Controls.Add(saleDateLabel, 0, 0);
            inputPanel.Controls.Add(_saleDatePicker, 1, 0);

            _saleStoreIdText = AddLabeledTextBox(inputPanel, "店舗ID", 2, 0);

            var productLabel = new Label {
                Text = "商品",
                Dock = DockStyle.Fill,
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft
            };
            _saleProductCombo = new ComboBox {
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            inputPanel.Controls.Add(productLabel, 0, 1);
            inputPanel.Controls.Add(_saleProductCombo, 1, 1);

            _saleQuantityText = AddLabeledTextBox(inputPanel, "数量", 2, 1);

            var unitPriceHeaderLabel = new Label {
                Text = "単価",
                Dock = DockStyle.Fill,
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft
            };
            _saleUnitPriceLabel = new Label {
                Dock = DockStyle.Fill,
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft
            };
            inputPanel.Controls.Add(unitPriceHeaderLabel, 0, 2);
            inputPanel.Controls.Add(_saleUnitPriceLabel, 1, 2);

            var amountHeaderLabel = new Label {
                Text = "売上金額見込",
                Dock = DockStyle.Fill,
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft
            };
            _saleAmountPreviewLabel = new Label {
                Dock = DockStyle.Fill,
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft
            };
            inputPanel.Controls.Add(amountHeaderLabel, 2, 2);
            inputPanel.Controls.Add(_saleAmountPreviewLabel, 3, 2);

            var buttonFlow = new FlowLayoutPanel {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = true
            };
            buttonFlow.Controls.Add(CreateButton("売上登録", RegisterSale));
            buttonFlow.Controls.Add(CreateButton("クリア", delegate { ClearSaleInputs(); }));
            inputPanel.Controls.Add(buttonFlow, 0, 3);
            inputPanel.SetColumnSpan(buttonFlow, 4);

            var hintLabel = new Label {
                Dock = DockStyle.Fill,
                Padding = new Padding(12, 0, 0, 0),
                Text = "商品選択で単価を表示します。数量は整数で入力してください。",
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft
            };

            _salesGrid = new DataGridView {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AutoGenerateColumns = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false
            };

            root.Controls.Add(inputPanel, 0, 0);
            root.Controls.Add(hintLabel, 0, 1);
            root.Controls.Add(_salesGrid, 0, 2);
            tab.Controls.Add(root);
            return tab;
        }

        private TabPage CreateAggregationTab() {
            var tab = new TabPage("売上集計");
            var root = new TableLayoutPanel {
                Dock = DockStyle.Fill,
                RowCount = 3,
                ColumnCount = 1
            };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 120));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            var inputPanel = new TableLayoutPanel {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 3,
                Padding = new Padding(12)
            };
            inputPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            inputPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            inputPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            inputPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

            var startLabel = new Label {
                Text = "開始日",
                Dock = DockStyle.Fill,
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft
            };
            _summaryStartDatePicker = new DateTimePicker {
                Dock = DockStyle.Fill,
                Format = DateTimePickerFormat.Short,
                Value = DateTime.Today.AddDays(-6)
            };
            inputPanel.Controls.Add(startLabel, 0, 0);
            inputPanel.Controls.Add(_summaryStartDatePicker, 1, 0);

            var endLabel = new Label {
                Text = "終了日",
                Dock = DockStyle.Fill,
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft
            };
            _summaryEndDatePicker = new DateTimePicker {
                Dock = DockStyle.Fill,
                Format = DateTimePickerFormat.Short,
                Value = DateTime.Today
            };
            inputPanel.Controls.Add(endLabel, 2, 0);
            inputPanel.Controls.Add(_summaryEndDatePicker, 3, 0);

            var buttonFlow = new FlowLayoutPanel {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = true
            };
            buttonFlow.Controls.Add(CreateButton("集計実行", ExecuteAggregation));
            buttonFlow.Controls.Add(CreateButton("集計結果コピー", CopyAggregationResult));
            inputPanel.Controls.Add(buttonFlow, 0, 1);
            inputPanel.SetColumnSpan(buttonFlow, 4);

            _summaryTotalLabel = new Label {
                Dock = DockStyle.Fill,
                Padding = new Padding(12, 0, 0, 0),
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft
            };

            var split = new SplitContainer {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Horizontal,
                SplitterDistance = 200
            };

            var productPanel = new TableLayoutPanel {
                Dock = DockStyle.Fill,
                RowCount = 2,
                ColumnCount = 1
            };
            productPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
            productPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            productPanel.Controls.Add(new Label {
                Text = "商品別集計",
                Dock = DockStyle.Fill,
                Padding = new Padding(8, 0, 0, 0),
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft
            }, 0, 0);

            _productSummaryGrid = new DataGridView {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AutoGenerateColumns = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false
            };
            productPanel.Controls.Add(_productSummaryGrid, 0, 1);

            var weeklyPanel = new TableLayoutPanel {
                Dock = DockStyle.Fill,
                RowCount = 2,
                ColumnCount = 1
            };
            weeklyPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
            weeklyPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            weeklyPanel.Controls.Add(new Label {
                Text = "週次集計",
                Dock = DockStyle.Fill,
                Padding = new Padding(8, 0, 0, 0),
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft
            }, 0, 0);

            _weeklySummaryGrid = new DataGridView {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AutoGenerateColumns = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false
            };
            weeklyPanel.Controls.Add(_weeklySummaryGrid, 0, 1);

            split.Panel1.Controls.Add(productPanel);
            split.Panel2.Controls.Add(weeklyPanel);

            root.Controls.Add(inputPanel, 0, 0);
            root.Controls.Add(_summaryTotalLabel, 0, 1);
            root.Controls.Add(split, 0, 2);
            tab.Controls.Add(root);
            return tab;
        }

        private static TextBox AddLabeledTextBox(TableLayoutPanel panel, string label, int col, int row) {
            var lbl = new Label {
                Text = label,
                Dock = DockStyle.Fill,
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft
            };

            var textBox = new TextBox { Dock = DockStyle.Fill };
            panel.Controls.Add(lbl, col, row);
            panel.Controls.Add(textBox, col + 1, row);
            return textBox;
        }

        private static Button CreateButton(string text, EventHandler onClick) {
            var button = new Button {
                Text = text,
                Width = 120,
                Height = 34,
                Margin = new Padding(0, 0, 12, 0)
            };
            button.Click += onClick;
            return button;
        }

        private void RegisterProduct(object sender, EventArgs e) {
            ExecuteWithValidation(delegate {
                _productService.Register(_products, BuildProductFromInput());
                RefreshProductsGrid();
                RefreshSaleProductOptions();
                RefreshSalesGrid();
                ClearProductInputs();
            });
        }

        private void UpdateProduct(object sender, EventArgs e) {
            ExecuteWithValidation(delegate {
                _productService.Update(_products, BuildProductFromInput());
                RefreshProductsGrid();
                RefreshSaleProductOptions();
                RefreshSalesGrid();
            });
        }

        private void DeleteProduct(object sender, EventArgs e) {
            var id = _productIdText.Text.Trim();
            if (string.IsNullOrWhiteSpace(id)) {
                MessageBox.Show("削除対象の商品IDを選択してください。", "入力エラー", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show(string.Format("商品ID={0} を削除します。よろしいですか？", id), "確認", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) {
                return;
            }

            ExecuteWithValidation(delegate {
                _productService.Delete(_products, id);
                RefreshProductsGrid();
                RefreshSaleProductOptions();
                RefreshSalesGrid();
                ClearProductInputs();
            });
        }

        private Product BuildProductFromInput() {
            int unitPrice;
            if (!int.TryParse(_unitPriceText.Text.Trim(), out unitPrice)) {
                throw new DomainValidationException("単価は整数で入力してください。");
            }

            return new Product {
                ProductId = _productIdText.Text.Trim(),
                ProductName = _productNameText.Text.Trim(),
                UnitPrice = unitPrice,
                Category = _categoryText.Text.Trim()
            };
        }

        private void RefreshProductsGrid() {
            _productsGrid.DataSource = null;
            _productsGrid.DataSource = _productService.GetAll(_products).Select(p => new {
                p.ProductId,
                p.ProductName,
                p.UnitPrice,
                p.Category
            }).ToList();
        }

        private void ProductsGridOnSelectionChanged(object sender, EventArgs e) {
            if (_productsGrid.SelectedRows.Count == 0) {
                return;
            }

            var row = _productsGrid.SelectedRows[0];
            _productIdText.Text = ToText(row.Cells["ProductId"].Value);
            _productNameText.Text = ToText(row.Cells["ProductName"].Value);
            _unitPriceText.Text = ToText(row.Cells["UnitPrice"].Value);
            _categoryText.Text = ToText(row.Cells["Category"].Value);
        }

        private void AddInventory(object sender, EventArgs e) {
            ExecuteWithValidation(delegate {
                var input = BuildInventoryInput();
                _inventoryService.AddStock(_inventories, input.StoreId, input.ProductId, input.Stock);
                RefreshInventoryGrid();
                ClearInventoryInputs();
                MessageBox.Show("入荷を反映しました。", "完了", MessageBoxButtons.OK, MessageBoxIcon.Information);
            });
        }

        private void RemoveInventory(object sender, EventArgs e) {
            ExecuteWithValidation(delegate {
                var input = BuildInventoryInput();
                _inventoryService.RemoveStock(_inventories, input.StoreId, input.ProductId, input.Stock);
                RefreshInventoryGrid();
                ClearInventoryInputs();
                MessageBox.Show("出庫を反映しました。", "完了", MessageBoxButtons.OK, MessageBoxIcon.Information);
            });
        }

        private InventoryRecord BuildInventoryInput() {
            int quantity;
            if (!int.TryParse(_inventoryQuantityText.Text.Trim(), out quantity)) {
                throw new DomainValidationException("数量は整数で入力してください。");
            }

            return new InventoryRecord {
                StoreId = _inventoryStoreIdText.Text.Trim(),
                ProductId = _inventoryProductIdText.Text.Trim(),
                Stock = quantity
            };
        }

        private void RefreshInventoryGrid() {
            _inventoryGrid.DataSource = null;
            _inventoryGrid.DataSource = _inventoryService.GetAll(_inventories).Select(r => new {
                r.StoreId,
                r.ProductId,
                r.Stock
            }).ToList();

            var reorderCount = _inventoryService.GetReorderTargets(_inventories, 5).Count;
            _reorderLabel.Text = string.Format("要発注（在庫5以下）件数: {0}", reorderCount);
        }

        private void InventoryGridOnSelectionChanged(object sender, EventArgs e) {
            if (_inventoryGrid.SelectedRows.Count == 0) {
                return;
            }

            var row = _inventoryGrid.SelectedRows[0];
            _inventoryStoreIdText.Text = ToText(row.Cells["StoreId"].Value);
            _inventoryProductIdText.Text = ToText(row.Cells["ProductId"].Value);
            _inventoryQuantityText.Text = string.Empty;
        }

        private void RegisterSale(object sender, EventArgs e) {
            if (!ValidateSaleInput(true)) {
                return;
            }

            ExecuteWithValidation(delegate {
                var input = BuildSaleInput();
                var registered = _salesService.RegisterSale(_sales, _products, _inventories, input);
                RefreshSalesGrid();
                RefreshInventoryGrid();
                ClearSaleInputs();
                MessageBox.Show(string.Format("売上を登録しました。金額: {0} 円", registered.SalesAmount), "完了", MessageBoxButtons.OK, MessageBoxIcon.Information);
            });
        }

        private SaleRecord BuildSaleInput() {
            var selected = _saleProductCombo.SelectedItem as SaleProductOption;
            if (selected == null) {
                throw new DomainValidationException("商品を選択してください。");
            }

            int quantity;
            if (!int.TryParse(_saleQuantityText.Text.Trim(), out quantity)) {
                throw new DomainValidationException("数量は整数で入力してください。");
            }

            return new SaleRecord {
                SaleDate = _saleDatePicker.Value.Date,
                StoreId = _saleStoreIdText.Text.Trim(),
                ProductId = selected.ProductId,
                Quantity = quantity
            };
        }

        private void RefreshSaleProductOptions() {
            var selectedId = GetSelectedSaleProductId();

            var options = _productService.GetAll(_products).Select(p => new SaleProductOption {
                ProductId = p.ProductId,
                ProductName = p.ProductName,
                UnitPrice = p.UnitPrice
            }).ToList();

            _saleProductCombo.DataSource = null;
            _saleProductCombo.DisplayMember = "DisplayText";
            _saleProductCombo.ValueMember = "ProductId";
            _saleProductCombo.DataSource = options;

            if (!string.IsNullOrWhiteSpace(selectedId)) {
                _saleProductCombo.SelectedValue = selectedId;
            }

            if (_saleProductCombo.Items.Count == 0) {
                _saleProductCombo.SelectedIndex = -1;
            }

            UpdateSaleUnitPriceAndAmountPreview();
            ValidateSaleInput(false);
        }

        private string GetSelectedSaleProductId() {
            var selected = _saleProductCombo == null ? null : _saleProductCombo.SelectedItem as SaleProductOption;
            return selected == null ? string.Empty : selected.ProductId;
        }

        private void RefreshSalesGrid() {
            var productNameMap = _products.ToDictionary(p => p.ProductId, p => p.ProductName);

            _salesGrid.DataSource = null;
            _salesGrid.DataSource = _salesService.GetAll(_sales).Select(s => new {
                SaleDate = s.SaleDate.ToString("yyyy/MM/dd"),
                s.StoreId,
                s.ProductId,
                ProductName = productNameMap.ContainsKey(s.ProductId) ? productNameMap[s.ProductId] : "(未登録商品)",
                s.Quantity,
                s.SalesAmount
            }).ToList();
        }

        private void WireSalesInputValidation() {
            _saleStoreIdText.TextChanged += delegate { ValidateSaleInput(false); };
            _saleQuantityText.TextChanged += delegate {
                ValidateSaleInput(false);
                UpdateSaleUnitPriceAndAmountPreview();
            };
            _saleProductCombo.SelectedIndexChanged += delegate {
                ValidateSaleInput(false);
                UpdateSaleUnitPriceAndAmountPreview();
            };
        }

        private bool ValidateSaleInput(bool showMessage) {
            var valid = true;

            if (string.IsNullOrWhiteSpace(_saleStoreIdText.Text)) {
                _errorProvider.SetError(_saleStoreIdText, "店舗IDを入力してください。");
                valid = false;
            } else {
                _errorProvider.SetError(_saleStoreIdText, string.Empty);
            }

            if (_saleProductCombo.SelectedItem == null) {
                _errorProvider.SetError(_saleProductCombo, "商品を選択してください。");
                valid = false;
            } else {
                _errorProvider.SetError(_saleProductCombo, string.Empty);
            }

            int quantity;
            if (string.IsNullOrWhiteSpace(_saleQuantityText.Text)) {
                _errorProvider.SetError(_saleQuantityText, "数量を入力してください。");
                valid = false;
            } else if (!int.TryParse(_saleQuantityText.Text.Trim(), out quantity)) {
                _errorProvider.SetError(_saleQuantityText, "数量は整数で入力してください。");
                valid = false;
            } else if (quantity <= 0) {
                _errorProvider.SetError(_saleQuantityText, "数量は1以上で入力してください。");
                valid = false;
            } else {
                _errorProvider.SetError(_saleQuantityText, string.Empty);
            }

            if (!valid && showMessage) {
                MessageBox.Show("入力内容を確認してください。", "入力エラー", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            return valid;
        }

        private void UpdateSaleUnitPriceAndAmountPreview() {
            var selected = _saleProductCombo.SelectedItem as SaleProductOption;
            if (selected == null) {
                _saleUnitPriceLabel.Text = "-";
                _saleAmountPreviewLabel.Text = "-";
                return;
            }

            _saleUnitPriceLabel.Text = string.Format("{0} 円", selected.UnitPrice);

            int quantity;
            if (!int.TryParse(_saleQuantityText.Text.Trim(), out quantity) || quantity <= 0) {
                _saleAmountPreviewLabel.Text = "-";
                return;
            }

            _saleAmountPreviewLabel.Text = string.Format("{0} 円", selected.UnitPrice * quantity);
        }

        private void ExecuteAggregation(object sender, EventArgs e) {
            ExecuteWithValidation(delegate {
                var snapshot = BuildAggregationSnapshot();
                RenderAggregationSnapshot(snapshot);
            });
        }

        private void CopyAggregationResult(object sender, EventArgs e) {
            ExecuteWithValidation(delegate {
                var snapshot = BuildAggregationSnapshot();
                RenderAggregationSnapshot(snapshot);
                Clipboard.SetText(BuildAggregationClipboardText(snapshot));
                MessageBox.Show("集計結果をクリップボードにコピーしました。", "完了", MessageBoxButtons.OK, MessageBoxIcon.Information);
            });
        }

        private AggregationSnapshot BuildAggregationSnapshot() {
            var startDate = _summaryStartDatePicker.Value.Date;
            var endDate = _summaryEndDatePicker.Value.Date;

            return new AggregationSnapshot {
                StartDate = startDate,
                EndDate = endDate,
                ProductSummaries = _salesAggregationService.GetProductSummaries(_sales, startDate, endDate).ToList(),
                WeeklySummaries = _salesAggregationService.GetWeeklySummaries(_sales, startDate, endDate).ToList(),
                TotalSalesAmount = _salesAggregationService.GetTotalSalesAmount(_sales, startDate, endDate)
            };
        }

        private void RenderAggregationSnapshot(AggregationSnapshot snapshot) {
            _summaryTotalLabel.Text = string.Format(
                "期間: {0:yyyy/MM/dd} - {1:yyyy/MM/dd} / 合計売上: {2} 円",
                snapshot.StartDate,
                snapshot.EndDate,
                snapshot.TotalSalesAmount);

            _productSummaryGrid.DataSource = null;
            _productSummaryGrid.DataSource = snapshot.ProductSummaries.Select(s => new {
                s.ProductId,
                s.TotalQuantity,
                s.TotalSalesAmount
            }).ToList();

            _weeklySummaryGrid.DataSource = null;
            _weeklySummaryGrid.DataSource = snapshot.WeeklySummaries.Select(s => new {
                Week = string.Format("{0:yyyy/MM/dd} - {1:yyyy/MM/dd}", s.WeekStartDate, s.WeekEndDate),
                s.TotalQuantity,
                s.TotalSalesAmount
            }).ToList();
        }

        private void ResetAggregationDisplay() {
            _summaryTotalLabel.Text = "期間を指定して集計を実行してください。";
            _productSummaryGrid.DataSource = null;
            _productSummaryGrid.DataSource = new List<object>();
            _weeklySummaryGrid.DataSource = null;
            _weeklySummaryGrid.DataSource = new List<object>();
        }

        private static string BuildAggregationClipboardText(AggregationSnapshot snapshot) {
            var builder = new StringBuilder();
            builder.AppendLine(string.Format("期間: {0:yyyy/MM/dd} - {1:yyyy/MM/dd}", snapshot.StartDate, snapshot.EndDate));
            builder.AppendLine(string.Format("合計売上: {0} 円", snapshot.TotalSalesAmount));
            builder.AppendLine();
            builder.AppendLine("[商品別集計]");
            builder.AppendLine("商品ID\t販売数量\t売上金額");

            foreach (var summary in snapshot.ProductSummaries) {
                builder.AppendLine(string.Format("{0}\t{1}\t{2}", summary.ProductId, summary.TotalQuantity, summary.TotalSalesAmount));
            }

            builder.AppendLine();
            builder.AppendLine("[週次集計]");
            builder.AppendLine("週\t販売数量\t売上金額");

            foreach (var summary in snapshot.WeeklySummaries) {
                builder.AppendLine(string.Format(
                    "{0:yyyy/MM/dd}-{1:yyyy/MM/dd}\t{2}\t{3}",
                    summary.WeekStartDate,
                    summary.WeekEndDate,
                    summary.TotalQuantity,
                    summary.TotalSalesAmount));
            }

            return builder.ToString();
        }

        private static string ToText(object value) {
            return value == null ? string.Empty : value.ToString();
        }

        private static void ExecuteWithValidation(Action action) {
            try {
                action.Invoke();
            } catch (DomainValidationException ex) {
                MessageBox.Show(ex.Message, "入力エラー", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            } catch (Exception ex) {
                MessageBox.Show(string.Format("予期しないエラーが発生しました: {0}", ex.Message), "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ClearProductInputs() {
            _productIdText.Text = string.Empty;
            _productNameText.Text = string.Empty;
            _unitPriceText.Text = string.Empty;
            _categoryText.Text = string.Empty;
        }

        private void ClearInventoryInputs() {
            _inventoryStoreIdText.Text = string.Empty;
            _inventoryProductIdText.Text = string.Empty;
            _inventoryQuantityText.Text = string.Empty;
        }

        private void ClearSaleInputs() {
            _saleDatePicker.Value = DateTime.Today;
            _saleStoreIdText.Text = string.Empty;
            _saleQuantityText.Text = string.Empty;
            if (_saleProductCombo.Items.Count > 0) {
                _saleProductCombo.SelectedIndex = 0;
            }
            ValidateSaleInput(false);
            UpdateSaleUnitPriceAndAmountPreview();
        }

        private class SaleProductOption {
            public string ProductId { get; set; }
            public string ProductName { get; set; }
            public int UnitPrice { get; set; }

            public string DisplayText {
                get { return string.Format("{0} - {1}", ProductId, ProductName); }
            }
        }

        private class AggregationSnapshot {
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public List<ProductSalesSummary> ProductSummaries { get; set; }
            public List<WeeklySalesSummary> WeeklySummaries { get; set; }
            public int TotalSalesAmount { get; set; }
        }
    }
}
