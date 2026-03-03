using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using SalesManagementApp.Core.Domain.Entities;

namespace Sales_Management_App {
    public partial class Form1 {
        private void BindInventoryHistoryOperationOptions() {
            var options = new List<InventoryHistoryOperationFilterOption> {
                new InventoryHistoryOperationFilterOption(string.Empty, "すべて"),
                new InventoryHistoryOperationFilterOption(InventoryOperationType.Inbound.ToString(), "入荷"),
                new InventoryHistoryOperationFilterOption(InventoryOperationType.Outbound.ToString(), "出庫"),
                new InventoryHistoryOperationFilterOption(InventoryOperationType.Sale.ToString(), "売上連動")
            };

            _historyOperationTypeCombo.DataSource = null;
            _historyOperationTypeCombo.DisplayMember = "Label";
            _historyOperationTypeCombo.ValueMember = "Value";
            _historyOperationTypeCombo.DataSource = options;
            _historyOperationTypeCombo.SelectedIndex = 0;
        }

        private void SearchInventoryHistory(object sender, EventArgs e) {
            ExecuteWithValidation(delegate {
                RefreshInventoryHistoryGrid();
            });
        }

        private void ClearInventoryHistoryFilter(object sender, EventArgs e) {
            _historyStartDatePicker.Checked = false;
            _historyEndDatePicker.Checked = false;
            _historyStoreIdText.Text = string.Empty;
            _historyProductIdText.Text = string.Empty;
            _historyOperationTypeCombo.SelectedIndex = 0;
            RefreshInventoryHistoryGrid();
        }

        private void RefreshInventoryHistoryGrid() {
            if (_inventoryHistoryGrid == null) {
                return;
            }

            var startDateTime = _historyStartDatePicker != null && _historyStartDatePicker.Checked
                ? _historyStartDatePicker.Value
                : (DateTime?)null;
            var endDateTime = _historyEndDatePicker != null && _historyEndDatePicker.Checked
                ? _historyEndDatePicker.Value
                : (DateTime?)null;
            var operationType = GetSelectedHistoryOperationType();

            var filtered = _inventoryHistoryService.Filter(
                _inventoryHistories,
                startDateTime,
                endDateTime,
                _historyStoreIdText == null ? string.Empty : _historyStoreIdText.Text,
                _historyProductIdText == null ? string.Empty : _historyProductIdText.Text,
                operationType);

            _inventoryHistoryGrid.DataSource = null;
            _inventoryHistoryGrid.DataSource = filtered.Select(h => new {
                OccurredAt = h.OccurredAt.ToString("yyyy/MM/dd HH:mm:ss"),
                OperationType = ToOperationTypeLabel(h.OperationType),
                h.StoreId,
                h.ProductId,
                h.Quantity,
                h.ResultStock,
                h.Result
            }).ToList();
            ApplyJapaneseHeaders(_inventoryHistoryGrid, InventoryHistoryGridHeaders);
        }

        private InventoryOperationType? GetSelectedHistoryOperationType() {
            var option = _historyOperationTypeCombo.SelectedItem as InventoryHistoryOperationFilterOption;
            if (option == null || string.IsNullOrWhiteSpace(option.Value)) {
                return null;
            }

            InventoryOperationType parsed;
            return Enum.TryParse(option.Value, out parsed) ? parsed : (InventoryOperationType?)null;
        }

        private static string ToOperationTypeLabel(InventoryOperationType operationType) {
            switch (operationType) {
                case InventoryOperationType.Inbound:
                    return "入荷";
                case InventoryOperationType.Outbound:
                    return "出庫";
                case InventoryOperationType.Sale:
                    return "売上連動";
                default:
                    return operationType.ToString();
            }
        }

        private void PersistInventoryHistory() {
            if (string.IsNullOrWhiteSpace(_inventoryHistoryPath)) {
                return;
            }

            _csvDataStore.WriteInventoryHistories(_inventoryHistoryPath, _inventoryHistories);
        }
    }
}
