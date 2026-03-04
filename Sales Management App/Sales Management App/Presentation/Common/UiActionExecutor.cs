using System;
using SalesManagementApp.Core.Application.Exceptions;

namespace Sales_Management_App.Presentation.Common {
    /// <summary>
    /// UiActionExecutor クラスです。
    /// </summary>
    internal sealed class UiActionExecutor {
        private readonly UiMessageService FMessageService;

        internal UiActionExecutor(UiMessageService messageService) {
            FMessageService = messageService ?? throw new ArgumentNullException("messageService");
        }

        internal void Execute(Action action) {
            try {
                action.Invoke();
            } catch (DomainValidationException ex) {
                FMessageService.ShowWarning(ex.Message, "入力エラー");
            } catch (Exception ex) {
                FMessageService.ShowError(ex);
            }
        }
    }
}
