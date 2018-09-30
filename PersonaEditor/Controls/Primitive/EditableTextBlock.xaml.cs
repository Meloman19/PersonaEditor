using System;
using System.Collections.Generic;
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
    public partial class EditableTextBlock : UserControl
    {
        public static DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(EditableTextBlock), new PropertyMetadata(""));

        public static DependencyProperty EditProperty = DependencyProperty.Register("Edit", typeof(bool), typeof(EditableTextBlock), new PropertyMetadata(false, EditPropertyChange));

        private static void EditPropertyChange(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is EditableTextBlock ed)
                ed.EditChange();
        }

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public bool Edit
        {
            get { return (bool)GetValue(EditProperty); }
            set { SetValue(EditProperty, value); }
        }

        public EditableTextBlock()
        {
            InitializeComponent();
            Main.DataContext = this;
        }

        private void EditChange()
        {
            if (Edit)
            {
                //   TBlock.Visibility = Visibility.Collapsed;
                //    TBox.Visibility = Visibility.Visible;
            }
            else
            {
                //    TBlock.Visibility = Visibility.Visible;
                //    TBox.Visibility = Visibility.Collapsed;
            }
        }

        private void UserControl_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
                TBlock.Foreground = new SolidColorBrush(Colors.Black);
            else
                TBlock.Foreground = new SolidColorBrush(Colors.Gray);
        }
    }
}
