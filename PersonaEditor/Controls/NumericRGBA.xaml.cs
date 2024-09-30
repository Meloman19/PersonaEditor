using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PersonaEditor.Controls
{
    public partial class NumericRGBA : UserControl
    {
        public event ColorChangeEventHandler ColorChanged;

        private event ColorChangeEventHandler ColorPropertyChanged;

        #region ColorProp
        public static readonly DependencyProperty ColorProperty = DependencyProperty.Register("Color", typeof(Color), typeof(NumericRGBA),
            new FrameworkPropertyMetadata(Colors.White, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, ColorPropertyCallback));

        [Bindable(true)]
        public Color Color
        {
            get { return (Color)GetValue(ColorProperty); }
            set { SetValue(ColorProperty, value); }
        }

        private static void ColorPropertyCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as NumericRGBA).ColorPropertyChanged?.Invoke((Color)e.NewValue);
        }
        #endregion ColorProp

        public NumericRGBA()
        {
            InitializeComponent();
            ColorPropertyChanged += NumericColorRGBA_ColorPropertyChanged;
            ColorA.ValueChanged += Color_ValueChanged;
            ColorR.ValueChanged += Color_ValueChanged;
            ColorG.ValueChanged += Color_ValueChanged;
            ColorB.ValueChanged += Color_ValueChanged;
        }

        private void NumericColorRGBA_ColorPropertyChanged(Color color)
        {
            ColorA.Value = color.A;
            ColorR.Value = color.R;
            ColorG.Value = color.G;
            ColorB.Value = color.B;
        }

        private void Color_ValueChanged(object sender, RoutedEventArgs e)
        {
            Color color = new Color()
            {
                A = Convert.ToByte(ColorA.Value),
                R = Convert.ToByte(ColorR.Value),
                G = Convert.ToByte(ColorG.Value),
                B = Convert.ToByte(ColorB.Value)
            };
            Color = color;
            ColorChanged?.Invoke(color);
        }
    }
}
