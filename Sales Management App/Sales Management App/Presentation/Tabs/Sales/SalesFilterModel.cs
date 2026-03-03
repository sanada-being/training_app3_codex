using System;

namespace Sales_Management_App.Presentation.Tabs.Sales {
    internal sealed class SalesFilterModel {
        internal DateTime? StartDate { get; set; }

        internal DateTime? EndDate { get; set; }

        internal string StoreId { get; set; }

        internal string ProductId { get; set; }
    }
}
