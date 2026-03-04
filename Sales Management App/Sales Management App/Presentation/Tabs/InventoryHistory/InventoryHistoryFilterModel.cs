using System;
using SalesManagementApp.Core.Domain.Entities;

namespace Sales_Management_App.Presentation.Tabs.InventoryHistory {
    /// <summary>
    /// 在庫履歴 タブで使用する検索条件を保持するモデルです。
    /// </summary>
    internal sealed class InventoryHistoryFilterModel {
        internal DateTime? StartDateTime { get; set; }

        internal DateTime? EndDateTime { get; set; }

        internal string StoreId { get; set; }

        internal string ProductId { get; set; }

        internal InventoryOperationType? OperationType { get; set; }
    }
}
