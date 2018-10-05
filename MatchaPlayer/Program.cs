using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MatchaPlayer
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            if (args.Count() != 0)
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new MainWindow(int.Parse(args[0])));
            }
            else MessageBox.Show("哈哈~这都被你发现了");
        }
    }
}
