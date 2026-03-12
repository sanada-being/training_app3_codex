using System;

namespace Sales_Management_App.Presentation.Tabs.Sales {
    /// <summary>
    /// 売上登録 タブで使用する入力値を保持するモデルです。
    /// </summary>
    internal sealed class SalesInputModel {
        internal DateTime SaleDate { get; set; }

        internal string StoreId { get; set; }

        internal string ProductId { get; set; }

        internal string QuantityText { get; set; }
    }
}
