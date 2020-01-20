using LemonLib;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace LemonApp
{
    public class MyScrollView : ScrollViewer
    {
        private int lastTimestamp = 0;
        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            if (Math.Abs(e.Delta) >= 80 && e.Timestamp-lastTimestamp >=100)
            {
                var sc = ScrollDirection.Up;
                if (e.Delta > 0)
                    sc = ScrollDirection.Down;
                SmoothScroll(sc);
                e.Handled = true;
            }
            else {
                var sc = ScrollDirection.Up;
                if (e.Delta > 0)
                    sc = ScrollDirection.Down;
                SmoothScroll(sc,false,e.Delta, e.Timestamp - lastTimestamp);
                e.Handled = true;
            }
            lastTimestamp = e.Timestamp;
        }
        private void SmoothScroll(ScrollDirection direction,bool needAni=true,int Delta=0,int time=0)
        {
            DoubleAnimation Animation = new DoubleAnimation();
            if (needAni)
            {
                Animation.EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut };
                if (ScrollDirection.Down == direction)
                    Animation.To = VerticalOffset - 200;
                else if (ScrollDirection.Up == direction)
                    Animation.To = VerticalOffset + 200;
                Animation.Duration = TimeSpan.FromMilliseconds(500);
            }
            else {
                Animation.To = VerticalOffset - Delta*1.5;
                Animation.Duration = TimeSpan.FromMilliseconds(0);
            }
            BeginAnimation(UIHelper.ScrollViewerBehavior.VerticalOffsetProperty, Animation);
        }
    }

    public enum ScrollDirection
    {
        Up, Down
    }

    public static class ListBoxInside{
        public static void Animation(this ListBox listbox,FrameworkElement ui) {
            int count = (int)(listbox.ActualHeight / 45);
            if (listbox.Items.IndexOf(ui) <= count)
            {
                ui.BeginAnimation(FrameworkElement.MarginProperty, new ThicknessAnimation(
                      new Thickness(0, 50, 0, -50), new Thickness(0), TimeSpan.FromSeconds(0.5))
                      { EasingFunction = new QuinticEase() { EasingMode = EasingMode.EaseOut } });
            }
        }
    }
}