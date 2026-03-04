using System;

namespace Sales_Management_App.Presentation.Tabs.Sales {
    /// <summary>
    /// SalesFilterModel クラスです。
    /// </summary>
    internal sealed class SalesFilterModel {
        internal DateTime? StartDate { get; set; }

        internal DateTime? EndDate { get; set; }

        internal string StoreId { get; set; }

        internal string ProductId { get; set; }
    }
}
