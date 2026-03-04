namespace Sales_Management_App.Presentation.Tabs.Products {
    /// <summary>
    /// 商品管理 タブで使用する入力値を保持するモデルです。
    /// </summary>
    internal sealed class ProductInputModel {
        internal string ProductId { get; set; }

        internal string ProductName { get; set; }

        internal string UnitPriceText { get; set; }

        internal string Category { get; set; }
    }
}
