using System;
using System.Windows.Forms;

namespace Sales_Management_App.Presentation.Common {
    /// <summary>
    /// UiMessageService クラスです。
    /// </summary>
    internal sealed class UiMessageService {
        internal void ShowInfo(string message, string title = "完了") {
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        internal void ShowWarning(string message, string title = "入力エラー") {
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        internal void ShowError(Exception ex) {
            MessageBox.Show(string.Format("予期しないエラーが発生しました: {0}", ex.Message), "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        internal DialogResult Confirm(string message, string title = "確認") {
            return MessageBox.Show(message, title, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        }
    }
}
