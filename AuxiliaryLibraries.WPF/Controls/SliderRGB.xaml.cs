using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace AuxiliaryLibraries.WPF.Controls
{
    public class RGBSliderToThumb : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            double value = (double)values[0];
            LinearGradientBrush linear = (LinearGradientBrush)values[1];

            int left = linear.GradientStops.IndexOf(linear.GradientStops.LastOrDefault(x => value >= x.Offset));

            if (left + 1 == linear.GradientStops.Count)
                return linear.GradientStops[left].Color;

            GradientStop before = linear.GradientStops[left];
            GradientStop after = linear.GradientStops[left + 1];

            var returned = new Color();

            returned.ScA = (float)((value - before.Offset) * (after.Color.ScA - before.Color.ScA) / (after.Offset - before.Offset) + before.Color.ScA);
            returned.ScR = (float)((value - before.Offset) * (after.Color.ScR - before.Color.ScR) / (after.Offset - before.Offset) + before.Color.ScR);
            returned.ScG = (float)((value - before.Offset) * (after.Color.ScG - before.Color.ScG) / (after.Offset - before.Offset) + before.Color.ScG);
            returned.ScB = (float)((value - before.Offset) * (after.Color.ScB - before.Color.ScB) / (after.Offset - before.Offset) + before.Color.ScB);

            return returned;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public partial class SliderRGB : UserControl
    {
        public static readonly DependencyProperty ColorProperty = DependencyProperty.Register("Color", typeof(Color), typeof(SliderRGB),
            new FrameworkPropertyMetadata(Colors.White, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, textChangedCallback));

        private static void textChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }

        [Bindable(true)]
        public Color Color
        {
            get { return (Color)GetValue(ColorProperty); }
            set { SetValue(ColorProperty, value); }
        }

        //public double Value
        //{
        //    get { return SliderRGBControl.Value; }
        //    // set { SliderRGBControl.Value = value; }
        //}
        public SliderRGB()
        {
            InitializeComponent();
            SetBind(this, ColorProperty, ColorSelect.FindResource("SliderThumb.Static.Background") as SolidColorBrush, "Color");
        }

        void SetBind(DependencyObject target, DependencyProperty depP, object obj, string propName)
        {
            Binding bind = new Binding(propName)
            {
                Source = obj
            };

            BindingOperations.SetBinding(target, depP, bind);
        }

    }
}