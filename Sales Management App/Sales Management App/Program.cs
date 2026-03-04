using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Sales_Management_App {
    /// <summary>
    /// WinFormsアプリケーションの起動エントリーポイントを提供します。
    /// </summary>
    internal static class Program {
        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        [STAThread]
        static void Main() {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            var dependencies = MainFormDependencies.CreateDefault();
            Application.Run(new Form1(dependencies));
        }
    }
}
