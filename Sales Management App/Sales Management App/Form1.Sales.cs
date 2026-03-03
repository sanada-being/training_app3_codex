using System;
using System.Linq;
using System.Windows.Forms;
using SalesManagementApp.Core.Application.Exceptions;
using SalesManagementApp.Core.Domain.Entities;

namespace Sales_Management_App {
    public partial class Form1 {
        private void RegisterSale(object sender, EventArgs e) {
            if (!ValidateSaleInput(true)) {
                return;
            }

            ExecuteWithValidation(delegate {
                var input = BuildSaleInput();
                var registered = _salesService.RegisterSale(_sales, _products, _inventories, input, _inventoryHistories, DateTime.Now);
                PersistInventoryHistory();
                RefreshSalesGrid();
                RefreshInventoryGrid();
                RefreshInventoryHistoryGrid();
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
    }
}
