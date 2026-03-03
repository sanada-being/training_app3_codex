namespace Sales_Management_App.Presentation.Tabs.Sales {
    internal sealed class SaleProductOption {
        public string ProductId { get; set; }

        public string ProductName { get; set; }

        public int UnitPrice { get; set; }

        public string DisplayText {
            get { return string.Format("{0} - {1}", ProductId, ProductName); }
        }
    }
}
