using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Lierda.WPFHelper
{
    public class LierdaCracker
    {
        private void SetDate()
        {
            CreateKey();
            RegistryKey expr_0B = Registry.CurrentUser;
            RegistryKey expr_17 = expr_0B.OpenSubKey("SOFTWARE\\DevExpress\\Components", true);
            expr_17.GetValue("LastAboutShowedTime");
            string value = DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss");
            expr_17.SetValue("LastAboutShowedTime", value);
            expr_0B.Dispose();
        }

        [DllImport("kernel32.dll")]
        private static extern bool SetProcessWorkingSetSize(IntPtr proc, int min, int max);

        private void FlushMemory()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                SetProcessWorkingSetSize(Process.GetCurrentProcess().Handle, -1, -1);
        }

        private void CreateKey()
        {
            RegistryKey currentUser = Registry.CurrentUser;
            if (currentUser.OpenSubKey("SOFTWARE\\DevExpress\\Components", true) == null)
            {
                RegistryKey expr_1F = currentUser.CreateSubKey("SOFTWARE\\DevExpress\\Components");
                expr_1F.CreateSubKey("LastAboutShowedTime").SetValue("LastAboutShowedTime", DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss"));
                expr_1F.CreateSubKey("DisableSmartTag").SetValue("LastAboutShowedTime", false);
                expr_1F.CreateSubKey("SmartTagWidth").SetValue("LastAboutShowedTime", 350);
            }
            currentUser.Dispose();
        }
        public bool Enabled { get; set; }
        public void Cracker(int sleepSpan = 50)
        {
            _ = Task.Factory.StartNew(delegate
            {
                while (true)
                {
                    try
                    {
                        if (Enabled)
                        {
                            SetDate();
                            FlushMemory();
                            Thread.Sleep(TimeSpan.FromSeconds((double)sleepSpan));
                        }
                    }
                    catch { }
                }
            });
        }
    }
}
