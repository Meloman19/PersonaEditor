using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace PersonaEditorGUI.Classes
{
    class ReadOnlyBinding
    {
        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.RegisterAttached("SelectedItem", typeof(object), typeof(ReadOnlyBinding), new PropertyMetadata(callback));

        private static void callback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }

        public static void SetSelectedItem(TreeView element, object value)
        {
            element.SetValue(SelectedItemProperty, value);
        }
        public static object GetSelectedItem(TreeView element)
        {
            return (DependencyProperty)element.GetValue(SelectedItemProperty);
        }
    }
}
