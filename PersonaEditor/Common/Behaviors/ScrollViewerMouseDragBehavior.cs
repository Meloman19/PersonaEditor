using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace PersonaEditor.Common.Behaviors
{
    internal class ScrollViewerMouseDragBehavior : Behavior<Decorator>
    {
        Point? scrollMousePoint;
        double horOff = 0;
        double verOff = 0;

        private IScrollInfo ScrollInfo => AssociatedObject as IScrollInfo;

        protected override void OnAttached()
        {
            if (ScrollInfo == null)
                return;

            AssociatedObject.PreviewMouseLeftButtonDown += AssociatedObject_PreviewMouseLeftButtonDown;
            AssociatedObject.PreviewMouseLeftButtonUp += AssociatedObject_PreviewMouseLeftButtonUp;
            AssociatedObject.PreviewMouseMove += AssociatedObject_PreviewMouseMove;
            AssociatedObject.LostMouseCapture += AssociatedObject_LostMouseCapture;
            AssociatedObject.MouseLeave += AssociatedObject_MouseLeave;
        }

        protected override void OnDetaching()
        {
            if (ScrollInfo == null)
                return;

            AssociatedObject.PreviewMouseLeftButtonDown -= AssociatedObject_PreviewMouseLeftButtonDown;
            AssociatedObject.PreviewMouseLeftButtonUp -= AssociatedObject_PreviewMouseLeftButtonUp;
            AssociatedObject.PreviewMouseMove -= AssociatedObject_PreviewMouseMove;
            AssociatedObject.LostMouseCapture -= AssociatedObject_LostMouseCapture;
            AssociatedObject.MouseLeave -= AssociatedObject_MouseLeave;
        }

        private void AssociatedObject_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            scrollMousePoint = e.GetPosition(AssociatedObject);
            horOff = ScrollInfo.HorizontalOffset;
            verOff = ScrollInfo.VerticalOffset;
        }

        private void AssociatedObject_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            AssociatedObject.ReleaseMouseCapture();
            LostCapture();
        }

        private void AssociatedObject_PreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (!scrollMousePoint.HasValue)
                return;

            var currentMousePos = e.GetPosition(AssociatedObject);
            var horDelta = scrollMousePoint.Value.X - currentMousePos.X;
            var verDelta = scrollMousePoint.Value.Y - currentMousePos.Y;

            if (AssociatedObject.IsMouseCaptured)
            {
                ScrollInfo.SetHorizontalOffset(horOff + horDelta);
                ScrollInfo.SetVerticalOffset(verOff + verDelta);
            }
            else
            {
                var rad = Math.Sqrt(Math.Abs(horDelta) * Math.Abs(horDelta) + Math.Abs(verDelta) * Math.Abs(verDelta));

                if (rad > 20)
                    AssociatedObject.CaptureMouse();
            }
        }

        private void AssociatedObject_LostMouseCapture(object sender, System.Windows.Input.MouseEventArgs e)
        {
            LostCapture();
        }

        private void AssociatedObject_MouseLeave(object sender, MouseEventArgs e)
        {
            if (AssociatedObject.IsMouseCaptured)
                return;

            LostCapture();
        }

        private void LostCapture()
        {
            scrollMousePoint = null;
        }
    }
}
