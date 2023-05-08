using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PersonaEditor.Controls
{
    internal static class AtlasHelper
    {
        public static void SetLocation(this FrameworkElement element, Rect location)
        {
            Canvas.SetLeft(element, location.Left);
            Canvas.SetTop(element, location.Top);
            element.Width = location.Width;
            element.Height = location.Height;
        }
    }

    internal class TextureAtlasEditControl : Control
    {
        public static readonly DependencyProperty ObjectBorderThicknessProperty =
            DependencyProperty.Register(nameof(ObjectBorderThickness), typeof(Thickness), typeof(TextureAtlasEditControl),
                new FrameworkPropertyMetadata(new Thickness(1)));

        public static readonly DependencyProperty TextureBitmapProperty =
            DependencyProperty.Register(nameof(TextureBitmap), typeof(BitmapSource), typeof(TextureAtlasEditControl),
                new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty ObjectListProperty =
            DependencyProperty.Register(nameof(ObjectList), typeof(IList), typeof(TextureAtlasEditControl),
                new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty SelectedObjectProperty =
            DependencyProperty.Register(nameof(SelectedObject), typeof(object), typeof(TextureAtlasEditControl),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        internal static readonly DependencyPropertyKey CursorPositionPropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(CursorPosition), typeof(Point?), typeof(TextureAtlasEditControl),
                new FrameworkPropertyMetadata(null, OnCursorPositionPropertyChanged));

        public static readonly DependencyProperty CursorPositionProperty
            = CursorPositionPropertyKey.DependencyProperty;

        private static void OnCursorPositionPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as TextureAtlasEditControl;
            var eventArgs = new RoutedEventArgs(CursorPositionChangedEvent);
            control.RaiseEvent(eventArgs);
        }

        public static readonly RoutedEvent CursorPositionChangedEvent =
            EventManager.RegisterRoutedEvent(nameof(CursorPositionChanged), RoutingStrategy.Direct, typeof(RoutedEventHandler), typeof(TextureAtlasEditControl));

        public event RoutedEventHandler CursorPositionChanged
        {
            add { AddHandler(CursorPositionChangedEvent, value); }
            remove { RemoveHandler(CursorPositionChangedEvent, value); }
        }

        private AtlasItemsContol _itemsControl;
        private readonly ScaleTransform _scaleTransform = new ScaleTransform();

        static TextureAtlasEditControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TextureAtlasEditControl),
                new FrameworkPropertyMetadata(typeof(TextureAtlasEditControl)));
        }

        public Thickness ObjectBorderThickness
        {
            get { return (Thickness)GetValue(ObjectBorderThicknessProperty); }
            set { SetValue(ObjectBorderThicknessProperty, value); }
        }

        public BitmapSource TextureBitmap
        {
            get { return (BitmapSource)GetValue(TextureBitmapProperty); }
            set { SetValue(TextureBitmapProperty, value); }
        }

        public IList ObjectList
        {
            get { return (IList)GetValue(ObjectListProperty); }
            set { SetValue(ObjectListProperty, value); }
        }

        public object SelectedObject
        {
            get { return GetValue(SelectedObjectProperty); }
            set { SetValue(SelectedObjectProperty, value); }
        }

        public Point? CursorPosition
        {
            get => (Point?)GetValue(CursorPositionProperty);
            private set => SetValue(CursorPositionPropertyKey, value);
        }

        public Size TexturePixelSize => TextureBitmap == null
            ? new Size()
            : new Size(TextureBitmap.PixelWidth, TextureBitmap.PixelHeight);

        protected override Size MeasureOverride(Size constraint)
        {
            Size realContstaint;

            double xScale = 1;
            double yScale = 1;
            var size = TexturePixelSize;
            if (size.Width == 0 || size.Height == 0)
                realContstaint = Size.Empty;
            else
            {
                xScale = constraint.Width / size.Width;
                var height = xScale * size.Height;
                if (height < constraint.Height)
                {
                    yScale = height / size.Height;
                    realContstaint = new Size(constraint.Width, height);
                }
                else
                {
                    yScale = constraint.Height / size.Height;
                    var width = yScale * size.Width;
                    xScale = width / size.Width;
                    realContstaint = new Size(width, constraint.Height);
                }
            }

            _scaleTransform.ScaleX = xScale;
            _scaleTransform.ScaleY = yScale;
            return base.MeasureOverride(realContstaint);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _itemsControl = GetTemplateChild("ItemsControl") as AtlasItemsContol;
            _itemsControl.RenderTransform = _scaleTransform;
            _itemsControl.CursorPositionChanged += (s, e) =>
            {
                CursorPosition = _itemsControl.CursorPosition;
            };
        }
    }
}