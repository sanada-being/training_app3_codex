using System;

namespace Sales_Management_App.Presentation.Tabs.Sales {
    internal sealed class SalesInputModel {
        internal DateTime SaleDate { get; set; }

        internal string StoreId { get; set; }

        internal string ProductId { get; set; }

        internal string QuantityText { get; set; }
    }
}
