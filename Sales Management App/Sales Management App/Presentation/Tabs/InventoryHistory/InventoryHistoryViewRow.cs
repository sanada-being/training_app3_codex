namespace Sales_Management_App.Presentation.Tabs.InventoryHistory {
    /// <summary>
    /// 在庫履歴 タブの一覧表示にバインドする行データモデルです。
    /// </summary>
    internal sealed class InventoryHistoryViewRow {
        /// <summary>
        /// 発生日時の表示文字列を取得または設定します。
        /// </summary>
        public string OccurredAt { get; set; }

        /// <summary>
        /// 操作種別の表示文字列を取得または設定します。
        /// </summary>
        public string OperationType { get; set; }

        /// <summary>
        /// 店舗IDを取得または設定します。
        /// </summary>
        public string StoreId { get; set; }

        /// <summary>
        /// 商品IDを取得または設定します。
        /// </summary>
        public string ProductId { get; set; }

        /// <summary>
        /// 操作数量を取得または設定します。
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// 操作後在庫数を取得または設定します。
        /// </summary>
        public int ResultStock { get; set; }

        /// <summary>
        /// 処理結果を取得または設定します。
        /// </summary>
        public string Result { get; set; }
    }
}
