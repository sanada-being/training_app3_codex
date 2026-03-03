using System;
using SalesManagementApp.Core.Application.Exceptions;

namespace Sales_Management_App {
    public partial class Form1 {
        private void LoadInitialDataFromRepositoryRoot() {
            try {
                _appState = _appBootstrapper.LoadFromBaseDirectory(AppDomain.CurrentDomain.BaseDirectory);
            } catch (DomainValidationException ex) {
                _messageService.ShowWarning(string.Format("初期データの読み込みに失敗しました: {0}", ex.Message), "入力エラー");
            } catch (Exception ex) {
                _messageService.ShowError(ex);
            }
        }
    }
}
