namespace Sales_Management_App.Presentation.Tabs.Inventory {
    /// <summary>
    /// 在庫管理 タブで使用する検索条件を保持するモデルです。
    /// </summary>
    internal sealed class InventoryFilterModel {
        internal string StoreId { get; set; }

        internal string ProductId { get; set; }
    }
}
