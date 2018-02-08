using PersonaEditorGUI.Classes;
using PersonaEditorLib.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
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

namespace PersonaEditorGUI.Controls
{
    public partial class SingleFileEdit : UserControl
    {
        public static readonly DependencyProperty FileOpenProperty = DependencyProperty.Register("FileOpen", typeof(bool), typeof(SingleFileEdit),
           new FrameworkPropertyMetadata(false));

        [Bindable(true)]
        public bool FileOpen
        {
            get { return (bool)GetValue(FileOpenProperty); }
            set { SetValue(FileOpenProperty, value); }
        }

        public SingleFileEdit()
        {
            InitializeComponent();
        }
        
        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue == null)
                FileOpen = false;
            else
                FileOpen = true;
        }
    }
}