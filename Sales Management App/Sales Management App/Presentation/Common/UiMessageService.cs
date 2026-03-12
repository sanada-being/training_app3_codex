using System;
using System.Windows.Forms;

namespace Sales_Management_App.Presentation.Common {
    /// <summary>
    /// 画面向けメッセージ表示を共通化するサービスです。
    /// </summary>
    internal sealed class UiMessageService {
        internal void ShowInfo(string vMessage, string vTitle = "完了") {
            MessageBox.Show(vMessage, vTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        internal void ShowWarning(string vMessage, string vTitle = "入力エラー") {
            MessageBox.Show(vMessage, vTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        internal void ShowError(Exception vEx) {
            MessageBox.Show(string.Format("予期しないエラーが発生しました: {0}", vEx.Message), "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        internal DialogResult Confirm(string vMessage, string vTitle = "確認") {
            return MessageBox.Show(vMessage, vTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        }
    }
}
