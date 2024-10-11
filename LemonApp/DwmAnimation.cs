using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interop;
using System.Windows;
using System.Windows.Threading;
using System.Reflection;

namespace LemonApp;

public static class DwmAnimation
{

    public static bool GetEnableDwmAnimation(DependencyObject obj)
    {
        return (bool)obj.GetValue(EnableDwmAnimationProperty);
    }

    public static void SetEnableDwmAnimation(DependencyObject obj, bool value)
    {
        obj.SetValue(EnableDwmAnimationProperty, value);
    }

    public static readonly DependencyProperty EnableDwmAnimationProperty =
        DependencyProperty.RegisterAttached("EnableDwmAnimation", 
            typeof(bool), typeof(DwmAnimation), 
            new PropertyMetadata(false,OnEnableDwmAnimationChanged));

    public static void OnEnableDwmAnimationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Window window)
        {
            if ((bool)e.NewValue)
            {
                if (window.IsLoaded)
                {
                    EnableDwmAnimation(window);
                }
                else
                {
                    window.SourceInitialized += Window_SourceInitialized;
                }
            }
        }
    }

    private static void Window_SourceInitialized(object? sender, EventArgs e)
    {
        if(sender is Window w)
        {
            EnableDwmAnimation(w);
            w.SourceInitialized -= Window_SourceInitialized;
        }
    }

    [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
    private static extern int SetWindowLong32(HandleRef hWnd, int nIndex, int dwNewLong);

    [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
    private static extern IntPtr SetWindowLongPtr64(HandleRef hWnd, int nIndex, IntPtr dwNewLong);

    public const int GWL_STYLE = -16;
    public const long WS_CAPTION = 0x00C00000L,
            WS_MAXIMIZEBOX = 0x00010000L,
        WS_MINIMIZEBOX = 0x00020000L,
        WS_THICKFRAME = 0x00040000L,
        WS_OVERLAPPED=	0x00000000L,
        WS_SYSMENU= 0x00080000L,
        WS_BORDER= 0x00800000L;

    public static IntPtr SetWindowLongPtr(HandleRef hWnd, int nIndex, IntPtr dwNewLong)
        => IntPtr.Size == 8 
        ? SetWindowLongPtr64(hWnd, nIndex, dwNewLong)
        : new IntPtr(SetWindowLong32(hWnd, nIndex, dwNewLong.ToInt32()));
    public static void EnableDwmAnimation(Window w)
    {
        var myHWND = new WindowInteropHelper(w).Handle;
        IntPtr myStyle = new(WS_CAPTION|WS_THICKFRAME|WS_MAXIMIZEBOX|WS_MINIMIZEBOX);
        if (w.ResizeMode == ResizeMode.NoResize||w.ResizeMode==ResizeMode.CanMinimize)
        {
            myStyle = new(WS_CAPTION | WS_MINIMIZEBOX);
        }
        SetWindowLongPtr(new HandleRef(null, myHWND), GWL_STYLE, myStyle);
    }
}
