using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace AuxiliaryLibraries.WPF.Controls
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
            ColorA.ValueChanged += ColorSet_Changed;
            ColorR.ValueChanged += ColorSet_Changed;
            ColorG.ValueChanged += ColorSet_Changed;
            ColorB.ValueChanged += ColorSet_Changed;
        }

        private void NumericColorRGBA_ColorPropertyChanged(Color color)
        {
            ColorA.Value = color.A;
            ColorR.Value = color.R;
            ColorG.Value = color.G;
            ColorB.Value = color.B;
        }

        private void ColorSet_Changed(double num)
        {
            Color color = new Color()
            {
                A = (byte)ColorA.Value,
                R = (byte)ColorR.Value,
                G = (byte)ColorG.Value,
                B = (byte)ColorB.Value
            };
            Color = color;
            ColorChanged?.Invoke(color);
        }
    }
}
