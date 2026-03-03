using System;
using System.Windows.Forms;
using SalesManagementApp.Core.Application.Exceptions;

namespace Sales_Management_App {
    public partial class Form1 {
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
    }
}
