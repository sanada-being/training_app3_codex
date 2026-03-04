using System;

namespace Sales_Management_App.Presentation.Tabs.Sales {
    /// <summary>
    /// 売上登録 タブで使用する検索条件を保持するモデルです。
    /// </summary>
    internal sealed class SalesFilterModel {
        internal DateTime? StartDate { get; set; }

        internal DateTime? EndDate { get; set; }

        internal string StoreId { get; set; }

        internal string ProductId { get; set; }
    }
}
