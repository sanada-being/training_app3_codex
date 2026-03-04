namespace Sales_Management_App.Presentation.Tabs.Sales {
    /// <summary>
    /// SalesViewRow クラスです。
    /// </summary>
    internal sealed class SalesViewRow {
        /// <summary>
        /// 売上日を取得または設定します。
        /// </summary>
        public string SaleDate { get; set; }

        /// <summary>
        /// 店舗IDを取得または設定します。
        /// </summary>
        public string StoreId { get; set; }

        /// <summary>
        /// 商品IDを取得または設定します。
        /// </summary>
        public string ProductId { get; set; }

        /// <summary>
        /// 商品名を取得または設定します。
        /// </summary>
        public string ProductName { get; set; }

        /// <summary>
        /// 数量を取得または設定します。
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// 売上金額を取得または設定します。
        /// </summary>
        public int SalesAmount { get; set; }
    }
}
