using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PersonaEditor.Classes
{
    class ReadOnlyPropertyObserver
    {
        public static DependencyProperty ActualHeightSenderPropery =
            DependencyProperty.RegisterAttached("ActualHeightSender",
                typeof(double), typeof(ReadOnlyPropertyObserver),
                new PropertyMetadata((double)0));

        public static DependencyProperty ActualHeightObserverPropery =
            DependencyProperty.RegisterAttached("ActualHeightObserver",
                typeof(double), typeof(ReadOnlyPropertyObserver),
                new PropertyMetadata(ActualHeightChangedCallback));

        private static void ActualHeightChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            d.SetValue(ActualHeightSenderPropery, e.NewValue);
        }

        public static void SetActualHeightSender(FrameworkElement element, double value)
        {
            element.SetValue(ActualHeightSenderPropery, value);
        }
        public static double GetActualHeightSender(FrameworkElement element)
        {
            return (double)element.GetValue(ActualHeightSenderPropery);
        }

        public static void SetActualHeightObserver(FrameworkElement element, double value)
        {
            element.SetValue(ActualHeightObserverPropery, value);
        }
        public static double GetActualHeightObserver(FrameworkElement element)
        {
            return (double)element.GetValue(ActualHeightObserverPropery);
        }
    }
}
