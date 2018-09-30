using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PersonaEditor.Controls.Primitive
{
    public delegate void NumberChangedEventHandler(double num);

    public partial class NumericIncDec : UserControl, INotifyPropertyChanged
    {
        #region INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;

        protected void Notify(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion INotifyPropertyChanged implementation

        public event NumberChangedEventHandler ValueChanged;
        public event NumberChangedEventHandler MaxChanged;
        public event NumberChangedEventHandler MinChanged;
        public event NumberChangedEventHandler DeltaChanged;

        #region NumberPropertys
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(double), typeof(NumericIncDec),
            new FrameworkPropertyMetadata((double)0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, ColorPropertyCallback));

        public static readonly DependencyProperty MaxValueProperty = DependencyProperty.Register("Max", typeof(double), typeof(NumericIncDec),
            new FrameworkPropertyMetadata((double)100, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, ColorPropertyCallback));

        public static readonly DependencyProperty MinValueProperty = DependencyProperty.Register("Min", typeof(double), typeof(NumericIncDec),
            new FrameworkPropertyMetadata((double)0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, ColorPropertyCallback));

        public static readonly DependencyProperty DeltaProperty = DependencyProperty.Register("Delta", typeof(double), typeof(NumericIncDec),
            new FrameworkPropertyMetadata((double)1, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, ColorPropertyCallback));


        [Bindable(true)]
        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        [Bindable(true)]
        public double Max
        {
            get { return (double)GetValue(MaxValueProperty); }
            set { SetValue(MaxValueProperty, value); }
        }

        [Bindable(true)]
        public double Min
        {
            get { return (double)GetValue(MinValueProperty); }
            set { SetValue(MinValueProperty, value); }
        }

        [Bindable(true)]
        public double Delta
        {
            get { return (double)GetValue(DeltaProperty); }
            set { SetValue(DeltaProperty, value); }
        }

        private static void ColorPropertyCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.Property.Name == "Value")
                (d as NumericIncDec).ValueChanged?.Invoke((double)e.NewValue);
            else if (e.Property.Name == "Max")
                (d as NumericIncDec).MaxChanged?.Invoke((double)e.NewValue);
            else if (e.Property.Name == "Min")
                (d as NumericIncDec).MinChanged?.Invoke((double)e.NewValue);
            else if (e.Property.Name == "Delta")
                (d as NumericIncDec).DeltaChanged?.Invoke((double)e.NewValue);
        }
        #endregion NumberPropertys

        private string _Text = "0";
        public string Text
        {
            get { return _Text; }
            set
            {
                if (value != _Text)
                    if (ValueValidate(value))
                    {
                        _Text = value;
                        Value = Double.Parse(value);

                    }
                Notify("Text");
            }
        }

        public NumericIncDec()
        {
            InitializeComponent();
            ValueChanged += NumericIncDec_ValueChanged;
        }

        private void NumericIncDec_ValueChanged(double num)
        {
            Text = Convert.ToString(num);
        }

        private bool ValueValidate(string num)
        {
            double temp;
            if (Double.TryParse(num, out temp))
                if (Min <= temp & temp <= Max)
                    return true;

            return false;
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up)
                ValueIncr();
            else if (e.Key == Key.Down)
                ValueDecr();
        }

        private void Increment_Click(object sender, RoutedEventArgs e)
        {
            ValueIncr();
        }

        private void Decrement_Click(object sender, RoutedEventArgs e)
        {
            ValueDecr();
        }

        private void ValueIncr()
        {
            Text = Convert.ToString(double.Parse(Text) + Delta);
        }

        private void ValueDecr()
        {
            Text = Convert.ToString(double.Parse(Text) - Delta);
        }
    }
}