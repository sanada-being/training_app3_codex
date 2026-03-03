namespace Sales_Management_App.Presentation.Tabs.InventoryHistory {
    internal sealed class InventoryHistoryViewRow {
        public string OccurredAt { get; set; }

        public string OperationType { get; set; }

        public string StoreId { get; set; }

        public string ProductId { get; set; }

        public int Quantity { get; set; }

        public int ResultStock { get; set; }

        public string Result { get; set; }
    }
}
