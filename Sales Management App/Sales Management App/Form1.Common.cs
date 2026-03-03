using System;

namespace Sales_Management_App {
    public partial class Form1 {
        private void ExecuteWithValidation(Action action) {
            _uiActionExecutor.Execute(action);
        }
    }
}
