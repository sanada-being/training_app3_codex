using System;
using System.Linq;
using System.Windows.Forms;
using SalesManagementApp.Core.Application.Exceptions;
using SalesManagementApp.Core.Domain.Entities;

namespace Sales_Management_App {
    public partial class Form1 {
        private void AddInventory(object sender, EventArgs e) {
            ExecuteWithValidation(delegate {
                var input = BuildInventoryInput();
                _inventoryService.AddStock(_inventories, input.StoreId, input.ProductId, input.Stock, _inventoryHistories, DateTime.Now);
                PersistInventoryHistory();
                RefreshInventoryGrid();
                RefreshInventoryHistoryGrid();
                ClearInventoryInputs();
                _messageService.ShowInfo("入荷を反映しました。");
            });
        }

        private void RemoveInventory(object sender, EventArgs e) {
            ExecuteWithValidation(delegate {
                var input = BuildInventoryInput();
                _inventoryService.RemoveStock(_inventories, input.StoreId, input.ProductId, input.Stock, _inventoryHistories, DateTime.Now);
                PersistInventoryHistory();
                RefreshInventoryGrid();
                RefreshInventoryHistoryGrid();
                ClearInventoryInputs();
                _messageService.ShowInfo("出庫を反映しました。");
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
            var filtered = _inventoryService.GetFiltered(
                _inventories,
                _inventoryFilterStoreIdText == null ? string.Empty : _inventoryFilterStoreIdText.Text,
                _inventoryFilterProductIdText == null ? string.Empty : _inventoryFilterProductIdText.Text);

            _inventoryGrid.DataSource = null;
            _inventoryGrid.DataSource = filtered.Select(r => new {
                r.StoreId,
                r.ProductId,
                r.Stock
            }).ToList();
            ApplyJapaneseHeaders(_inventoryGrid, InventoryGridHeaders);

            var reorderCount = _inventoryService.GetReorderTargets(_inventories, 5).Count;
            _reorderLabel.Text = string.Format("要発注（在庫5以下）件数: {0}", reorderCount);
        }

        private void SearchInventories(object sender, EventArgs e) {
            RefreshInventoryGrid();
        }

        private void ClearInventoryFilter(object sender, EventArgs e) {
            _inventoryFilterStoreIdText.Text = string.Empty;
            _inventoryFilterProductIdText.Text = string.Empty;
            RefreshInventoryGrid();
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
    }
}
