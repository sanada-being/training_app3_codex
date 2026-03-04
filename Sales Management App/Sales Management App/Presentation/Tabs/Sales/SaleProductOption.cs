namespace Sales_Management_App.Presentation.Tabs.Sales {
    /// <summary>
    /// 売上登録時の商品選択候補を表すモデルです。
    /// </summary>
    internal sealed class SaleProductOption {
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
        /// コンボボックス表示用の文字列を取得します。
        /// </summary>
        public string DisplayText {
            get { return string.Format("{0} - {1}", this.ProductId, this.ProductName); }
        }
    }
}
