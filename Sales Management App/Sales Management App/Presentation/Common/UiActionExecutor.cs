using System;
using SalesManagementApp.Core.Application.Exceptions;

namespace Sales_Management_App.Presentation.Common {
    internal sealed class UiActionExecutor {
        private readonly UiMessageService _messageService;

        internal UiActionExecutor(UiMessageService messageService) {
            _messageService = messageService ?? throw new ArgumentNullException("messageService");
        }

        internal void Execute(Action action) {
            try {
                action.Invoke();
            } catch (DomainValidationException ex) {
                _messageService.ShowWarning(ex.Message, "入力エラー");
            } catch (Exception ex) {
                _messageService.ShowError(ex);
            }
        }
    }
}
