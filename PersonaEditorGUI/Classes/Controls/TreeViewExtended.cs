using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace PersonaEditorGUI.Classes.Controls
{
    class TreeViewExtended : TreeView
    {
        public new static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(
                "SelectedItem",
                typeof(object),
                typeof(TreeViewExtended),
                new PropertyMetadata(null));

        private static void callback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }

        public TreeViewExtended() : base()
        {
            SelectedItemChanged += TreeViewExtended_SelectedItemChanged;
        }

        private void TreeViewExtended_SelectedItemChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<object> e)
        {
            SetValue(SelectedItemProperty, e.NewValue);
            
        }

        public new object SelectedItem
        {
            get { return GetValue(SelectedItemProperty); }
            set { }
        }
    }
}