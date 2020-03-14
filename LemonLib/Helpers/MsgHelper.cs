using System;
using System.Runtime.InteropServices;
using System.Text;

namespace LemonLib
{
    public class MsgHelper
    {
        public const int WM_COPYDATA = 0x004A;
        public const string SEND_SHOW = "SEND_SHOW";

        public struct COPYDATASTRUCT
        {
            public IntPtr dwData;
            public int cData;
            [MarshalAs(UnmanagedType.LPStr)]
            public string lpData;
        }

        [DllImport("User32.dll", EntryPoint = "FindWindow")]
        public extern static IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("User32.dll")]
        public static extern int SendMessage(int hwnd, int msg, int wParam, ref COPYDATASTRUCT IParam);

        public static async void SendMsg(String strSent, int WindowHandle = 0)
        {
            int WINDOW_HANDLE = WindowHandle;
            if (WindowHandle == 0)
                WINDOW_HANDLE = (await Settings.ReadHandleAsync()).WINDOW_HANDLE;
            if (WINDOW_HANDLE != 0)
            {
                byte[] arr = Encoding.Default.GetBytes(strSent);
                int len = arr.Length;
                COPYDATASTRUCT cdata;
                cdata.dwData = (IntPtr)100;
                cdata.lpData = strSent;
                cdata.cData = len + 1;
                SendMessage(WINDOW_HANDLE, WM_COPYDATA, 0, ref cdata);
            }
        }
    }
}
