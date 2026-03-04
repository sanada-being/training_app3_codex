namespace Sales_Management_App.Presentation.Tabs.Aggregation {
    /// <summary>
    /// 売上集計の商品別サマリー行を表すモデルです。
    /// </summary>
    internal sealed class AggregationProductSummaryRow {
        /// <summary>
        /// 商品IDを取得または設定します。
        /// </summary>
        public string ProductId { get; set; }

        /// <summary>
        /// 合計数量を取得または設定します。
        /// </summary>
        public int TotalQuantity { get; set; }

        /// <summary>
        /// 合計売上金額を取得または設定します。
        /// </summary>
        public int TotalSalesAmount { get; set; }
    }
}
