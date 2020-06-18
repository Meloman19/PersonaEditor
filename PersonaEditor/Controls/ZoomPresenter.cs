using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace PersonaEditor.Controls
{
    class ZoomPresenter : Decorator, IScrollInfo, IScrollInfo2
    {
        internal const double _scrollLineDelta = 16.0;
        internal const double _mouseWheelDelta = 48.0;

        private Size Extent;
        private Size Viewport;
        private Point Offset;

        private TransformGroup childRenderTransform;

        public ZoomPresenter()
        {
            Scale = new ScaleTransform();
            Translate = new TranslateTransform();

            childRenderTransform = new TransformGroup();
            childRenderTransform.Children.Add(Scale);
            childRenderTransform.Children.Add(Translate);
        }

        protected override void OnVisualChildrenChanged(DependencyObject visualAdded, DependencyObject visualRemoved)
        {
            if (visualRemoved is UIElement oldChild)
            {
                oldChild.RenderTransform = null;
            }

            if (visualAdded is UIElement newChild)
            {
                newChild.RenderTransform = childRenderTransform;
            }

            base.OnVisualChildrenChanged(visualAdded, visualRemoved);
        }

        private ScaleTransform Scale { get; }
        private TranslateTransform Translate { get; }

        #region IScrollInfo

        public bool CanVerticallyScroll { get; set; }
        public bool CanHorizontallyScroll { get; set; }

        public double ExtentWidth => Extent.Width * ZoomFactor;

        public double ExtentHeight => Extent.Height * ZoomFactor;

        public double ViewportWidth => Viewport.Width;

        public double ViewportHeight => Viewport.Height;

        public double HorizontalOffset => Offset.X;

        public double VerticalOffset => Offset.Y;

        public ScrollViewer ScrollOwner { get; set; }

        public void LineUp()
        {
        }

        public void LineDown()
        {
        }

        public void LineLeft()
        {
        }

        public void LineRight()
        {
        }

        public void PageUp()
        {
        }

        public void PageDown()
        {
        }

        public void PageLeft()
        {
        }

        public void PageRight()
        {
        }

        public void MouseWheelUp()
        {
            SetVerticalOffset(VerticalOffset - SystemParameters.WheelScrollLines * _scrollLineDelta);
        }

        public void MouseWheelDown()
        {
            SetVerticalOffset(VerticalOffset + SystemParameters.WheelScrollLines * _scrollLineDelta);
        }

        public void MouseWheelLeft()
        {
            SetHorizontalOffset(HorizontalOffset - SystemParameters.WheelScrollLines * _scrollLineDelta);
        }

        public void MouseWheelRight()
        {
            SetHorizontalOffset(HorizontalOffset + SystemParameters.WheelScrollLines * _scrollLineDelta);
        }

        public void SetHorizontalOffset(double offset)
        {
            if (ExtentWidth <= ViewportWidth)
            {
                offset = 0;
            }
            else
            {
                var available = ExtentWidth - ViewportWidth;
                if (offset < 0)
                {
                    offset = 0;
                }
                else if (offset > available)
                {
                    offset = available;
                }
            }

            Offset = new Point(offset, Offset.Y);
            ScrollOwner?.InvalidateScrollInfo();

            SetHorizontalTranslate(offset);
        }

        private void SetHorizontalTranslate(double offset)
        {
            double translateX;
            if (ExtentWidth <= ViewportWidth)
            {
                translateX = (ViewportWidth - ExtentWidth) / 2;
            }
            else
            {
                translateX = -offset;
            }

            Translate.X = translateX;
        }

        public void SetVerticalOffset(double offset)
        {
            if (ExtentHeight <= ViewportHeight)
            {
                offset = 0;
            }
            else
            {
                var available = ExtentHeight - ViewportHeight;
                if (offset < 0)
                {
                    offset = 0;
                }
                else if (offset > available)
                {
                    offset = available;
                }
            }

            Offset = new Point(Offset.X, offset);
            ScrollOwner?.InvalidateScrollInfo();

            SetVerticalTranslate(offset);
        }

        private void SetVerticalTranslate(double offset)
        {
            double translateY;
            if (ExtentHeight <= ViewportHeight)
            {
                translateY = (ViewportHeight - ExtentHeight) / 2;
            }
            else
            {
                translateY = -offset;
            }

            Translate.Y = translateY;
        }

        public Rect MakeVisible(Visual visual, Rect rectangle)
        {
            return rectangle;
        }

        #endregion

        #region IZoomInfo

        private double ZoomFactor = 1;

        public void ZoomTo(Point point, double zoomFactor)
        {
            if(ZoomFactor == zoomFactor)
            {
                return;
            }

            var offsetY = ((Offset.Y + point.Y) / ZoomFactor) * zoomFactor - point.Y;
            var offsetX = ((Offset.X + point.X) / ZoomFactor) * zoomFactor - point.X;

            ZoomFactor = zoomFactor;

            Scale.ScaleX = zoomFactor;
            Scale.ScaleY = zoomFactor;
            ScrollOwner?.InvalidateScrollInfo();

            SetVerticalOffset(offsetY);
            SetHorizontalOffset(offsetX);
        }

        #endregion

        protected override Size MeasureOverride(Size availableSize)
        {
            if(double.IsInfinity(availableSize.Width)
                || double.IsInfinity(availableSize.Height))
            {
                return new Size();
            }

            Child.Measure(availableSize);

            Extent = Child.DesiredSize;
            Viewport = availableSize;

            SetHorizontalOffset(Offset.X);
            SetVerticalOffset(Offset.Y);

            ScrollOwner?.InvalidateScrollInfo();

            return availableSize;
        }

        protected override Size ArrangeOverride(Size arrangeSize)
        {
            Child.Arrange(new Rect(new Point(), Child.DesiredSize));

            return arrangeSize;
        }
    }
}
