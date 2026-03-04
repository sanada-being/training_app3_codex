namespace Sales_Management_App.Presentation.Tabs.Products {
    /// <summary>
    /// 商品管理 タブで使用する検索条件を保持するモデルです。
    /// </summary>
    internal sealed class ProductFilterModel {
        internal string ProductId { get; set; }

        internal string ProductName { get; set; }

        internal string Category { get; set; }
    }
}
