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
        private readonly List<Product> _products = new List<Product>();

        private DataGridView _productsGrid;
        private TextBox _productIdText;
        private TextBox _productNameText;
        private TextBox _unitPriceText;
        private TextBox _categoryText;

        public Form1() {
            InitializeComponent();
            InitializeProductScreen();
        }

        private void InitializeProductScreen() {
            Text = "Sales Management App";
            Width = 1000;
            Height = 650;

            var root = new TableLayoutPanel {
                Dock = DockStyle.Fill,
                RowCount = 2,
                ColumnCount = 1
            };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 170));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            Controls.Add(root);

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

            RefreshProductsGrid();
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
            _productIdText.Text = row.Cells["ProductId"].Value == null ? string.Empty : row.Cells["ProductId"].Value.ToString();
            _productNameText.Text = row.Cells["ProductName"].Value == null ? string.Empty : row.Cells["ProductName"].Value.ToString();
            _unitPriceText.Text = row.Cells["UnitPrice"].Value == null ? string.Empty : row.Cells["UnitPrice"].Value.ToString();
            _categoryText.Text = row.Cells["Category"].Value == null ? string.Empty : row.Cells["Category"].Value.ToString();
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
    }
}
