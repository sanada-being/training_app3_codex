namespace Sales_Management_App.Presentation.Tabs.InventoryHistory {
    /// <summary>
    /// 在庫履歴の操作種別フィルター項目を表すモデルです。
    /// </summary>
    internal sealed class InventoryHistoryOperationFilterOption {
        /// <summary>
        /// フィルタ選択肢を初期化します。
        /// </summary>
        /// <param name="value">選択肢の内部値。</param>
        /// <param name="label">画面表示ラベル。</param>
        public InventoryHistoryOperationFilterOption(string vValue, string vLabel) {
            this.Value = vValue;
            this.Label = vLabel;
        }

        /// <summary>
        /// 選択肢の内部値を取得します。
        /// </summary>
        public string Value { get; private set; }

        /// <summary>
        /// 選択肢の表示ラベルを取得します。
        /// </summary>
        public string Label { get; private set; }
    }
}
