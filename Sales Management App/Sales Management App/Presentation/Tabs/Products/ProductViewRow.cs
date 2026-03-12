namespace Sales_Management_App.Presentation.Tabs.Products {
    /// <summary>
    /// 商品管理 タブの一覧表示にバインドする行データモデルです。
    /// </summary>
    internal sealed class ProductViewRow {
        /// <summary>
        /// 商品IDを取得または設定します。
        /// </summary>
        public string ProductId { get; set; }

        /// <summary>
        /// 商品名を取得または設定します。
        /// </summary>
        public string ProductName { get; set; }

        /// <summary>
        /// 単価を取得または設定します。
        /// </summary>
        public int UnitPrice { get; set; }

        /// <summary>
        /// カテゴリを取得または設定します。
        /// </summary>
        public string Category { get; set; }
    }
}
