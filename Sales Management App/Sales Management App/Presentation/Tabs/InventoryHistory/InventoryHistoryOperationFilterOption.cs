namespace Sales_Management_App.Presentation.Tabs.InventoryHistory {
    internal sealed class InventoryHistoryOperationFilterOption {
        public InventoryHistoryOperationFilterOption(string value, string label) {
            Value = value;
            Label = label;
        }

        public string Value { get; private set; }

        public string Label { get; private set; }
    }
}
