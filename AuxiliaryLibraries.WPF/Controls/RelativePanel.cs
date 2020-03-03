using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace AuxiliaryLibraries.WPF.Controls
{
    public class RelativePanel : Panel
    {
        public static readonly DependencyProperty ItemRectProperty =
            DependencyProperty.RegisterAttached("ItemRect", typeof(Rect), typeof(RelativePanel), new PropertyMetadata(Rect.Empty, ItemSizeCallback));

        public static readonly DependencyProperty RectProperty =
            DependencyProperty.Register("Rect", typeof(Rect), typeof(RelativePanel), new PropertyMetadata(Rect.Empty, PanelRectCallback));

        [Bindable(true)]
        public Rect Rect
        {
            get { return (Rect)GetValue(RectProperty); }
            set { SetValue(RectProperty, value); }
        }

        public static Rect GetItemRect(DependencyObject obj)
        {
            return (Rect)obj.GetValue(ItemRectProperty);
        }

        public static void SetItemRect(DependencyObject obj, double value)
        {
            obj.SetValue(ItemRectProperty, value);
        }

        private static void ItemSizeCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is UIElement child)
                if (VisualTreeHelper.GetParent(child) is RelativePanel panel)
                    panel.InvalidateMeasure();
        }

        private static void PanelRectCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is RelativePanel panel)
                panel.InvalidateMeasure();
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            if (Rect != Rect.Empty)
            {
                var a = availableSize.Width / availableSize.Height;
                var b = Rect.Width / Rect.Height;

                Size newsize;
                if (double.IsNaN(b))
                    newsize = new Size();
                else
                    newsize = a > b ? new Size(b * availableSize.Height, availableSize.Height) : new Size(availableSize.Width, availableSize.Width / b);

                foreach (UIElement child in this.Children)
                    child.Measure(ControlSize(child, newsize, Rect));

                return newsize;
            }
            else
            {
                var rect = new Size();
                foreach (UIElement child in this.Children)
                    child.Measure(rect);
                return rect;
            }
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            foreach (UIElement child in this.Children)
            {
                if (Rect != Rect.Empty)
                {
                    var itemrect = GetItemRect(child);

                    var x = itemrect.Left * finalSize.Width / Rect.Width;
                    var y = itemrect.Top * finalSize.Height / Rect.Height;

                    if (itemrect != Rect.Empty)
                        child.Arrange(new Rect(new Point(x, y), child.DesiredSize));
                }
            }

            return finalSize;
        }

        private static Size ControlSize(UIElement uIElement, Size newsize, Rect rect)
        {
            if (rect != Rect.Empty)
            {
                var itemrect = GetItemRect(uIElement);

                if (itemrect != Rect.Empty)
                {
                    var width = itemrect.Width * newsize.Width / rect.Width;
                    var height = itemrect.Height * newsize.Height / rect.Height;

                    if (double.IsNaN(width) | double.IsNaN(height))
                        return new Size();
                    else
                        return new Size(width, height);
                }
            }

            return newsize;
        }
    }
}