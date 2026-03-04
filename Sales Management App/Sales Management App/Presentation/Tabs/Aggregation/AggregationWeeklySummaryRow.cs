namespace Sales_Management_App.Presentation.Tabs.Aggregation {
    /// <summary>
    /// AggregationWeeklySummaryRow クラスです。
    /// </summary>
    internal sealed class AggregationWeeklySummaryRow {
        /// <summary>
        /// 週の表示文字列を取得または設定します。
        /// </summary>
        public string Week { get; set; }

        /// <summary>
        /// 週の合計数量を取得または設定します。
        /// </summary>
        public int TotalQuantity { get; set; }

        /// <summary>
        /// 週の合計売上金額を取得または設定します。
        /// </summary>
        public int TotalSalesAmount { get; set; }
    }
}
