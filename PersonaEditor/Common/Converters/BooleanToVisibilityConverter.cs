using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace PersonaEditor.Common.Converters
{
    public sealed class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolean)
                return boolean ? Visibility.Visible : Visibility.Collapsed;

            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility visibility)
                return visibility == Visibility.Visible ? true : false;

            return DependencyProperty.UnsetValue;
        }
    }
}