using LemonLib;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace LemonApp
{
    /// <summary>
    /// 全局ScrollView 行为 滚动
    /// </summary>
    public class MyScrollView : ScrollViewer
    {
        private double LastLocation = 0;
        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            double WheelChange = e.Delta;
            double newOffset = LastLocation - (WheelChange * 2);
            ScrollToVerticalOffset(LastLocation);
            if (newOffset < 0)
                newOffset = 0;
            if (newOffset > ScrollableHeight)
                newOffset = ScrollableHeight;

            AnimateScroll(newOffset);
            LastLocation = newOffset;
            e.Handled = true;
        }
        private void AnimateScroll(double ToValue)
        {
            BeginAnimation(UIHelper.ScrollViewerBehavior.VerticalOffsetProperty, null);
            DoubleAnimation Animation = new DoubleAnimation();
            Animation.EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut };
            Animation.From = VerticalOffset;
            Animation.To = ToValue;
            Animation.Duration = TimeSpan.FromMilliseconds(800);
            //Timeline.SetDesiredFrameRate(Animation, 40);
            BeginAnimation(UIHelper.ScrollViewerBehavior.VerticalOffsetProperty, Animation);
        }
    }
}