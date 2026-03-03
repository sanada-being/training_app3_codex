using System;
using System.Linq;
using System.Windows.Forms;
using SalesManagementApp.Core.Application.Exceptions;
using SalesManagementApp.Core.Domain.Entities;

namespace Sales_Management_App {
    public partial class Form1 {
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
            var filtered = _productService.GetFiltered(
                _products,
                _productFilterIdText == null ? string.Empty : _productFilterIdText.Text,
                _productFilterNameText == null ? string.Empty : _productFilterNameText.Text,
                _productFilterCategoryText == null ? string.Empty : _productFilterCategoryText.Text);

            _productsGrid.DataSource = null;
            _productsGrid.DataSource = filtered.Select(p => new {
                p.ProductId,
                p.ProductName,
                p.UnitPrice,
                p.Category
            }).ToList();
            ApplyJapaneseHeaders(_productsGrid, ProductGridHeaders);
        }

        private void SearchProducts(object sender, EventArgs e) {
            RefreshProductsGrid();
        }

        private void ClearProductFilter(object sender, EventArgs e) {
            _productFilterIdText.Text = string.Empty;
            _productFilterNameText.Text = string.Empty;
            _productFilterCategoryText.Text = string.Empty;
            RefreshProductsGrid();
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
    }
}
