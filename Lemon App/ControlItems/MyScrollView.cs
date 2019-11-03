using LemonLibrary;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace Lemon_App
{
    public class MyScrollView : ScrollViewer
    {
        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            if (Math.Abs(e.Delta) >= 80)
            {
                var sc = ScrollDirection.Up;
                if (e.Delta > 0)
                    sc = ScrollDirection.Down;
                SmoothScroll(sc);
                e.Handled = true;
            }
            else {
                base.OnMouseWheel(e);
            }
        }
        private void SmoothScroll(ScrollDirection direction)
        {
            DoubleAnimation Animation = new DoubleAnimation();
            Animation.EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut};
            if (ScrollDirection.Down == direction)
            {
                Animation.To = VerticalOffset - 200;
            }
            else if (ScrollDirection.Up == direction)
            {
                Animation.To = VerticalOffset +200;
            }
            Animation.Duration = new Duration(TimeSpan.FromMilliseconds(500));
            BeginAnimation(UIHelper.ScrollViewerBehavior.VerticalOffsetProperty, Animation);
        }
    }

    public enum ScrollDirection
    {
        Up, Down
    }
}