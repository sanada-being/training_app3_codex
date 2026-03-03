using System;
using SalesManagementApp.Core.Application.Exceptions;

namespace Sales_Management_App {
    public partial class Form1 {
        private void LoadInitialDataFromRepositoryRoot() {
            try {
                var loaded = _appBootstrapper.LoadFromBaseDirectory(AppDomain.CurrentDomain.BaseDirectory);
                _appState.Products = loaded.Products;
                _appState.Inventories = loaded.Inventories;
                _appState.InventoryHistories = loaded.InventoryHistories;
                _appState.Sales = loaded.Sales;
                _appState.RepositoryRootPath = loaded.RepositoryRootPath;
                _appState.ProductsPath = loaded.ProductsPath;
                _appState.InventoryPath = loaded.InventoryPath;
                _appState.SalesPath = loaded.SalesPath;
                _appState.InventoryHistoryPath = loaded.InventoryHistoryPath;
            } catch (DomainValidationException ex) {
                _messageService.ShowWarning(string.Format("初期データの読み込みに失敗しました: {0}", ex.Message), "入力エラー");
            } catch (Exception ex) {
                _messageService.ShowError(ex);
            }
        }
    }
}
