using System;

namespace Sales_Management_App {
    public partial class Form1 {
        private static string ToText(object value) {
            return value == null ? string.Empty : value.ToString();
        }

        private void ExecuteWithValidation(Action action) {
            _uiActionExecutor.Execute(action);
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
