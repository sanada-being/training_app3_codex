using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using SalesManagementApp.Core.Application.Exceptions;
using SalesManagementApp.Core.Application.Services;
using SalesManagementApp.Core.Domain.Entities;

namespace Sales_Management_App {
    public partial class Form1 : Form {
        private readonly ProductService _productService = new ProductService();
        private readonly InventoryService _inventoryService = new InventoryService();

        private readonly List<Product> _products = new List<Product>();
        private readonly List<InventoryRecord> _inventories = new List<InventoryRecord>();

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

        public Form1() {
            InitializeComponent();
            InitializeMainTabs();
        }

        private void InitializeMainTabs() {
            Text = "Sales Management App";
            Width = 1100;
            Height = 700;

            var tabs = new TabControl { Dock = DockStyle.Fill };
            tabs.TabPages.Add(CreateProductTab());
            tabs.TabPages.Add(CreateInventoryTab());
            Controls.Add(tabs);

            RefreshProductsGrid();
            RefreshInventoryGrid();
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
                Width = 110,
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
                ClearProductInputs();
            });
        }

        private void UpdateProduct(object sender, EventArgs e) {
            ExecuteWithValidation(delegate {
                _productService.Update(_products, BuildProductFromInput());
                RefreshProductsGrid();
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
    }
}
