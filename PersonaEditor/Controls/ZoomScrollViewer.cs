using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace PersonaEditor.Controls
{
    public sealed class ZoomScrollViewer : ScrollViewer
    {
        private const double MaxZoomFactor = 15;
        private const double MinZoomFactor = 1;

        private double _zoomFactor;

        private double ZoomFactor
        {
            get { return _zoomFactor; }
            set
            {
                if (value < MinZoomFactor)
                {
                    _zoomFactor = MinZoomFactor;
                }
                else if (value > MaxZoomFactor)
                {
                    _zoomFactor = MaxZoomFactor;
                }
                else
                {
                    _zoomFactor = value;
                }
            }
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                ZoomMouseWheel(e);
            }
            else if (Keyboard.Modifiers == ModifierKeys.Shift)
            {
                HorizontalMouseScroll(e);
            }
            else
            {
                base.OnMouseWheel(e);
            }
        }

        private void ZoomMouseWheel(MouseWheelEventArgs e)
        {
            if (e.Handled)
                return;

            var scrollInfo2 = Content as IScrollInfo2;
            if (scrollInfo2 == null)
                return;

            if (e.Delta < 0)
                ZoomFactor -= 0.3;
            else
                ZoomFactor += 0.3;

            var point = new Point();
            if (Content is IInputElement ie)
            {
                point = e.GetPosition(ie);
            }

            scrollInfo2.ZoomTo(point, ZoomFactor);

            e.Handled = true;
        }

        private void HorizontalMouseScroll(MouseWheelEventArgs e)
        {
            if (e.Handled)
                return;

            var scrollInfo = Content as IScrollInfo;
            if (scrollInfo == null)
                return;

            if (e.Delta < 0)
            {
                scrollInfo.MouseWheelRight();
            }
            else
            {
                scrollInfo.MouseWheelLeft();
            }

            e.Handled = true;
        }

        public void SetToDefault()
        {
            ZoomFactor = 1;

            var scrollInfo2 = Content as IScrollInfo2;
            if (scrollInfo2 == null)
                return;

            var frameworkElement = Content as FrameworkElement;
            if (frameworkElement == null)
                return;

            scrollInfo2.ZoomTo(new Point(frameworkElement.ActualWidth / 2, frameworkElement.ActualHeight / 2), ZoomFactor);
        }
    }
}