using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace PersonaEditor.Controls
{
    public sealed class NumericUpDown : Control
    {
        private TextBox NumericTextBox;
        private RepeatButton NumericUp;
        private RepeatButton NumericDown;

        private bool _supressTextBoxUpdate = false;

        public static readonly RoutedEvent ValueChangedEvent = EventManager.RegisterRoutedEvent(
            name: nameof(ValueChanged),
            routingStrategy: RoutingStrategy.Direct,
            handlerType: typeof(RoutedEventHandler),
            ownerType: typeof(NumericUpDown));

        public static readonly DependencyProperty MinValueProperty =
            DependencyProperty.Register(nameof(MinValue), typeof(double), typeof(NumericUpDown), new FrameworkPropertyMetadata(0d));

        public static readonly DependencyProperty MaxValueProperty =
            DependencyProperty.Register(nameof(MaxValue), typeof(double), typeof(NumericUpDown), new FrameworkPropertyMetadata(1d));

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(nameof(Value), typeof(double?), typeof(NumericUpDown), new FrameworkPropertyMetadata(null, ValuePropertyChanged, CoerceValueProperty));

        public static readonly DependencyProperty IsIntegerOnlyProperty =
            DependencyProperty.Register(nameof(IsIntegerOnly), typeof(bool), typeof(NumericUpDown), new FrameworkPropertyMetadata(false));

        private static void ValuePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (NumericUpDown)d;
            control.OnValuePropertyChanged((double?)e.OldValue, (double?)e.NewValue);
        }

        private static object CoerceValueProperty(DependencyObject d, object baseValue)
        {
            var control = (NumericUpDown)d;
            var min = control.MinValue;
            var max = control.MaxValue;
            double? val = (double?)baseValue;

            if (!val.HasValue)
                return val;

            if (val < min)
                return min;
            if (val > max)
                return max;
            return val;
        }

        static NumericUpDown()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(NumericUpDown),
                new FrameworkPropertyMetadata(typeof(NumericUpDown)));
        }

        public event RoutedEventHandler ValueChanged
        {
            add { AddHandler(ValueChangedEvent, value); }
            remove { RemoveHandler(ValueChangedEvent, value); }
        }

        public double MinValue
        {
            get { return (double)GetValue(MinValueProperty); }
            set { SetValue(MinValueProperty, value); }
        }

        public double MaxValue
        {
            get { return (double)GetValue(MaxValueProperty); }
            set { SetValue(MaxValueProperty, value); }
        }

        public double? Value
        {
            get { return (double?)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public bool IsIntegerOnly
        {
            get { return (bool)GetValue(IsIntegerOnlyProperty); }
            set { SetValue(IsIntegerOnlyProperty, value); }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            NumericTextBox = GetTemplateChild(nameof(NumericTextBox)) as TextBox;
            NumericTextBox.TextChanged += NumericTextBox_TextChanged;
            UpdateTextWithSupressing(Value);

            NumericUp = GetTemplateChild(nameof(NumericUp)) as RepeatButton;
            NumericUp.Click += NumericUp_Click;
            NumericDown = GetTemplateChild(nameof(NumericDown)) as RepeatButton;
            NumericDown.Click += NumericDown_Click;
        }

        private void OnValuePropertyChanged(double? oldValue, double? newValue)
        {
            UpdateTextWithSupressing(newValue);
            RaiseValueChangedEvent();
        }

        private void NumericTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_supressTextBoxUpdate)
                return;

            var newText = NumericTextBox.Text;

            if (string.IsNullOrEmpty(newText))
            {
                SetCurrentValue(ValueProperty, null);
            }
            else if (double.TryParse(newText, out var newValue))
            {
                if (IsIntegerOnly)
                {
                    if (int.TryParse(newText, out _))
                    {
                        SetCurrentValue(ValueProperty, newValue);
                    }
                    else
                    {
                        UpdateTextWithSupressing(Value);
                    }
                }
                else
                {
                    SetCurrentValue(ValueProperty, newValue);
                }
            }
            else
            {
                UpdateTextWithSupressing(Value);
            }
        }

        private void UpdateTextWithSupressing(double? val)
        {
            if (NumericTextBox == null)
                return;

            try
            {
                _supressTextBoxUpdate = true;
                var caret = NumericTextBox.CaretIndex;
                NumericTextBox.Text = val?.ToString() ?? string.Empty;
                NumericTextBox.CaretIndex = caret;
            }
            finally
            {
                _supressTextBoxUpdate = false;
            }
        }

        private void NumericUp_Click(object sender, RoutedEventArgs e)
        {
            var value = Value ?? 0;
            SetCurrentValue(ValueProperty, value + 1);
        }

        private void NumericDown_Click(object sender, RoutedEventArgs e)
        {
            var value = Value ?? 0;
            SetCurrentValue(ValueProperty, value - 1);
        }

        private void RaiseValueChangedEvent()
        {
            RoutedEventArgs routedEventArgs = new(routedEvent: ValueChangedEvent);
            RaiseEvent(routedEventArgs);
        }
    }
}