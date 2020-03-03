using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace AuxiliaryLibraries.WPF.Controls
{
    public delegate void ColorChangeEventHandler(Color color);
    public partial class CanvasRGB : UserControl
    {
        public event ColorChangeEventHandler BaseColorChanged;
        public event ColorChangeEventHandler SelectColorChanged;

        #region ColorProp
        public static readonly DependencyProperty ColorProperty = DependencyProperty.Register("Color", typeof(Color), typeof(CanvasRGB),
            new FrameworkPropertyMetadata(Colors.White, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, ColorPropertyCallback));

        [Bindable(true)]
        public Color Color
        {
            get { return (Color)GetValue(ColorProperty); }
            set { SetValue(ColorProperty, value); }
        }

        private static void ColorPropertyCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as CanvasRGB).BaseColorChanged?.Invoke((Color)e.NewValue);
        }
        #endregion ColorProp

        #region SelColorProp
        public static readonly DependencyProperty SelectColorProperty = DependencyProperty.Register("SelectedColor", typeof(Color), typeof(CanvasRGB),
            new FrameworkPropertyMetadata(Colors.White, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, SelectColorPropertyCallback));

        [Bindable(true)]
        public Color SelectedColor
        {
            get { return (Color)GetValue(SelectColorProperty); }
            set { SetValue(SelectColorProperty, value); }
        }

        private static void SelectColorPropertyCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as CanvasRGB).SelectColorChanged?.Invoke((Color)e.NewValue);
        }
        #endregion SelColorProp

        private Point CurrentPosition { get; set; } = new Point(0, 1);

        public CanvasRGB()
        {
            BaseColorChanged += CanvasRGB_BaseColorChanged;
            SelectColorChanged += CanvasRGB_SelectColorChanged;
            InitializeComponent();
            BackgroundImage.SizeChanged += BackgroundImage_SizeChanged;

            SelectColor.DataContext = this;
        }

        private void CanvasRGB_SelectColorChanged(Color color)
        {
        }

        private void BackgroundImage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Canvas.SetLeft(SelectColor, CurrentPosition.X * e.NewSize.Width - 15);
            Canvas.SetTop(SelectColor, (1 - CurrentPosition.Y) * e.NewSize.Height - 15);
            GetCurrentColor(Color);
        }

        private void CanvasRGB_BaseColorChanged(Color color)
        {
            GetCurrentColor(color);
        }

        private void ColorCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var point = e.GetPosition(sender as Canvas);
                Canvas.SetLeft(SelectColor, point.X - 15);
                Canvas.SetTop(SelectColor, point.Y - 15);
                CurrentPosition = new Point(point.X / BackgroundImage.ActualWidth, 1 - point.Y / BackgroundImage.ActualHeight);
                GetCurrentColor(Color);
            }
        }

        private void ColorCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var point = e.GetPosition(sender as Canvas);
            Canvas.SetLeft(SelectColor, point.X - 15);
            Canvas.SetTop(SelectColor, point.Y - 15);
            CurrentPosition = new Point(point.X / BackgroundImage.ActualWidth, 1 - point.Y / BackgroundImage.ActualHeight);
            GetCurrentColor(Color);
        }

        private void GetCurrentColor(Color basecolor)
        {
            Color returned = GetGradientColor(new Color() { A = 0xFF, R = 0xFF, G = 0xFF, B = 0xFF }, basecolor, CurrentPosition.X);
            returned = GetGradientColor(new Color() { A = 0xFF, R = 0, G = 0, B = 0 }, returned, CurrentPosition.Y);
            SelectedColor = returned;
        }

        private Color GetGradientColor(Color before, Color after, double offset)
        {
            var returned = new Color() { A = 0xFF };

            returned.ScR = (float)(offset * (after.ScR - before.ScR) + before.ScR);
            returned.ScG = (float)(offset * (after.ScG - before.ScG) + before.ScG);
            returned.ScB = (float)(offset * (after.ScB - before.ScB) + before.ScB);

            return returned;
        }
    }
}