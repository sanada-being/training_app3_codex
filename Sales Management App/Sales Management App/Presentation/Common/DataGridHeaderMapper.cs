using System.Collections.Generic;
using System.Windows.Forms;

namespace Sales_Management_App.Presentation.Common {
    internal static class DataGridHeaderMapper {
        internal static void Apply(DataGridView grid, IReadOnlyDictionary<string, string> headers) {
            if (grid == null || headers == null) {
                return;
            }

            foreach (DataGridViewColumn column in grid.Columns) {
                string mapped;
                if (headers.TryGetValue(column.Name, out mapped)) {
                    column.HeaderText = mapped;
                }
            }
        }
    }
}
