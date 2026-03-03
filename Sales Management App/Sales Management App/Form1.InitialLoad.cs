using System;
using System.Windows.Forms;
using SalesManagementApp.Core.Application.Exceptions;

namespace Sales_Management_App {
    public partial class Form1 {
        private void LoadInitialDataFromRepositoryRoot() {
            try {
                _appState = _appBootstrapper.LoadFromBaseDirectory(AppDomain.CurrentDomain.BaseDirectory);

                RefreshProductsGrid();
                RefreshInventoryGrid();
                RefreshInventoryHistoryGrid();
                RefreshSaleProductOptions();
                RefreshSalesGrid();
                ResetAggregationDisplay();
                UpdateSaleUnitPriceAndAmountPreview();
            } catch (DomainValidationException ex) {
                MessageBox.Show(string.Format("初期データの読み込みに失敗しました: {0}", ex.Message), "入力エラー", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            } catch (Exception ex) {
                MessageBox.Show(string.Format("初期データの読み込み中にエラーが発生しました: {0}", ex.Message), "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
