namespace Sales_Management_App.Presentation.Tabs.Inventory {
    /// <summary>
    /// 在庫管理 タブの一覧表示にバインドする行データモデルです。
    /// </summary>
    internal sealed class InventoryViewRow {
        /// <summary>
        /// 店舗IDを取得または設定します。
        /// </summary>
        public string StoreId { get; set; }

        /// <summary>
        /// 商品IDを取得または設定します。
        /// </summary>
        public string ProductId { get; set; }

        /// <summary>
        /// 在庫数を取得または設定します。
        /// </summary>
        public int Stock { get; set; }
    }
}
