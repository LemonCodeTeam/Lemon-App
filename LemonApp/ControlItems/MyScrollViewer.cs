using LemonLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace LemonApp
{
    /// <summary>
    /// 全局ScrollViewer 行为 滚动
    /// </summary>
    public class MyScrollViewer : ScrollViewer
    {
        public MyScrollViewer()
        {
            this.FocusVisualStyle = null;
            this.PreviewMouseUp += MyScrollView_PreviewMouseUp;
            this.PanningMode= PanningMode.VerticalOnly;
            this.SnapsToDevicePixels = true;
        }

        private void MyScrollView_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            //响应鼠标操作:  手动滑动滚动条的时候更新位置
            LastLocation = VerticalOffset;
        }

        public double LastLocation = 0;
        public new void ScrollToVerticalOffset(double offset)
        {
            if (Settings.USettings.Animation_Scroll)
            {
                AnimateScroll(offset);
                LastLocation = offset;
            }
            else
            {
                base.ScrollToVerticalOffset(offset);
            }
        }
        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            if (Settings.USettings.Animation_Scroll)
            {
                double newOffset = LastLocation - e.Delta;
                base.ScrollToVerticalOffset(LastLocation);
                if (newOffset < 0)
                    newOffset = 0;
                if (newOffset > ScrollableHeight)
                    newOffset = ScrollableHeight;
                AnimateScroll(newOffset);
                LastLocation = newOffset;
                e.Handled = true;
            }
            else base.OnMouseWheel(e);
        }

        private void AnimateScroll(double ToValue)
        {
            BeginAnimation(UIHelper.ScrollViewerBehavior.VerticalOffsetProperty, null);
            DoubleAnimation Animation = new DoubleAnimation();
            Animation.EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut };
            Animation.From = VerticalOffset;
            Animation.To = ToValue;
            Animation.Duration = TimeSpan.FromMilliseconds(300);
            //Timeline.SetDesiredFrameRate(Animation, 40);
            BeginAnimation(UIHelper.ScrollViewerBehavior.VerticalOffsetProperty, Animation);
        }
    }
}