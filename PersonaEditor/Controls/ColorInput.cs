using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using PersonaEditor.Views.Tools;

namespace PersonaEditor.Controls
{
    public sealed class ColorInput : Control
    {
        private TextBlock ColorTextBlock;
        private Button ColorSelectButton;

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(nameof(Value), typeof(Color?), typeof(ColorInput), new FrameworkPropertyMetadata(null, ValuePropertyChanged));

        private static void ValuePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (ColorInput)d;
            control.OnValuePropertyChanged((Color?)e.OldValue, (Color?)e.NewValue);
        }

        static ColorInput()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ColorInput),
                new FrameworkPropertyMetadata(typeof(ColorInput)));
        }

        public Color? Value
        {
            get { return (Color?)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            ColorSelectButton = GetTemplateChild(nameof(ColorSelectButton)) as Button;
            ColorSelectButton.Click += ColorSelectButton_Click;

            ColorTextBlock = GetTemplateChild(nameof(ColorTextBlock)) as TextBlock;
            UpdateText();
        }

        private void ColorSelectButton_Click(object sender, RoutedEventArgs e)
        {
            var tool = new ColorPickerTool()
            {
                SelectedColor = Value ?? Colors.White,
            };
            if (tool.ShowDialog() == true)
                SetCurrentValue(ValueProperty, tool.SelectedColor);
        }

        private void OnValuePropertyChanged(Color? oldValue, Color? newValue)
        {
            UpdateText();
        }

        private void UpdateText()
        {
            if (ColorTextBlock == null)
                return;

            ColorTextBlock.Text = Value?.ToString() ?? string.Empty;
        }
    }
}