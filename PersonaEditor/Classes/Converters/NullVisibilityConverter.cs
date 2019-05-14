using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace PersonaEditor.Classes.Converters
{
    public class NullVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((string)parameter == "Reverse")
            {
                return value == null ? Visibility.Visible : Visibility.Collapsed;
            }
            else if ((string)parameter == "Bool")
            {
                return (bool)value == true ? Visibility.Visible : Visibility.Collapsed;
            }
            else if ((string)parameter == "BoolReverse")
            {
                return (bool)value == true ? Visibility.Collapsed : Visibility.Visible;
            }
            else if ((string)parameter == "ItemsSource")
            {
                if (value != null)
                    if ((value as System.Collections.ICollection).Count != 0)
                        return Visibility.Collapsed;
                return Visibility.Visible;
            }
            else if ((string)parameter == "ItemsSourceReverse")
            {
                if (value != null)
                    if ((value as System.Collections.ICollection).Count != 0)
                        return Visibility.Visible;
                return Visibility.Collapsed;
            }
            return value == null ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
