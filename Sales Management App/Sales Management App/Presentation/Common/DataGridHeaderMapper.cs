using System.Collections.Generic;
using System.Windows.Forms;

namespace Sales_Management_App.Presentation.Common {
    /// <summary>
    /// DataGridView列ヘッダーの表示名マッピングを適用します。
    /// </summary>
    internal static class DataGridHeaderMapper {
        internal static void Apply(DataGridView vGrid, IReadOnlyDictionary<string, string> vHeaders) {
            if (vGrid == null || vHeaders == null) {
                return;
            }

            foreach (DataGridViewColumn wColumn in vGrid.Columns) {
                string wMapped;
                if (vHeaders.TryGetValue(wColumn.Name, out wMapped)) {
                    wColumn.HeaderText = wMapped;
                }
            }
        }
    }
}
