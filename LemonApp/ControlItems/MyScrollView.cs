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
            var sc = ScrollDirection.Up;
            if (e.Delta > 0)
                sc = ScrollDirection.Down;
            int time = e.Timestamp - lastTimestamp;
            int move = Math.Abs(e.Delta);
            //较缓慢地滚动
            if (time >= 80)
            {
                SmoothScroll(sc, move);
            }
            //较快地滚动
            else
            {
                //触屏滚动(无需动画)
                if (move<20&& time <30)
                    SmoothScrollinTouch(e.Delta);
                else
                    SmoothScrollSpeed2x(sc);
            }
            lastTimestamp = e.Timestamp;
        }
        private void SmoothScroll(ScrollDirection direction, int move)
        {
            DoubleAnimation Animation = new DoubleAnimation();
            Animation.EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut };
            if (ScrollDirection.Down == direction)
                Animation.To = VerticalOffset - move;
            else if (ScrollDirection.Up == direction)
                Animation.To = VerticalOffset + move;
            Animation.Duration = TimeSpan.FromMilliseconds(500);
            BeginAnimation(UIHelper.ScrollViewerBehavior.VerticalOffsetProperty, Animation);
        }
        bool beginning = false;
        private void SmoothScrollSpeed2x(ScrollDirection direction)
        {
            if (!beginning)
            {
                DoubleAnimation Animation = new DoubleAnimation();
                Animation.EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut };
                if (ScrollDirection.Down == direction)
                    Animation.To = VerticalOffset - 400;
                else if (ScrollDirection.Up == direction)
                    Animation.To = VerticalOffset + 400;
                Animation.Duration = TimeSpan.FromMilliseconds(300);
                Animation.Completed += delegate {
                    beginning = false; 
                };
                beginning = true;
                BeginAnimation(UIHelper.ScrollViewerBehavior.VerticalOffsetProperty, Animation);
            }
        }
        private void SmoothScrollinTouch(int Delta)
        {
            DoubleAnimation Animation = new DoubleAnimation();
            Animation.To = VerticalOffset - Delta;
            Animation.Duration = TimeSpan.FromMilliseconds(0);
            BeginAnimation(UIHelper.ScrollViewerBehavior.VerticalOffsetProperty, Animation);
        }
    }

    public enum ScrollDirection
    {
        Up, Down
    }

    public static class ListBoxInside
    {
        public static void Animation(this ListBox listbox, FrameworkElement ui)
        {
            int count = (int)(listbox.ActualHeight / 45);
            if (listbox.Items.IndexOf(ui) <= count)
            {
                var ani = new ThicknessAnimation(
                      new Thickness(0, 50, 0, -50), new Thickness(0), TimeSpan.FromSeconds(0.3))
                { EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut } };
                ui.BeginAnimation(FrameworkElement.MarginProperty, ani);
            }
        }
    }
}