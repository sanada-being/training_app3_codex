using System;
using SalesManagementApp.Core.Application.Exceptions;

namespace Sales_Management_App.Presentation.Common {
    /// <summary>
    /// UIイベント処理の例外ハンドリングを統一します。
    /// </summary>
    internal sealed class UiActionExecutor {
        private readonly UiMessageService FMessageService;

        internal UiActionExecutor(UiMessageService vMessageService) {
            FMessageService = vMessageService ?? throw new ArgumentNullException("messageService");
        }

        internal void Execute(Action vAction) {
            try {
                vAction.Invoke();
            } catch (DomainValidationException wEx) {
                FMessageService.ShowWarning(wEx.Message, "入力エラー");
            } catch (Exception wEx) {
                FMessageService.ShowError(wEx);
            }
        }
    }
}
