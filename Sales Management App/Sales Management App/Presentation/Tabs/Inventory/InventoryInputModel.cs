namespace Sales_Management_App.Presentation.Tabs.Inventory {
    /// <summary>
    /// 在庫管理 タブで使用する入力値を保持するモデルです。
    /// </summary>
    internal sealed class InventoryInputModel {
        internal string StoreId { get; set; }

        internal string ProductId { get; set; }

        internal string QuantityText { get; set; }
    }
}
