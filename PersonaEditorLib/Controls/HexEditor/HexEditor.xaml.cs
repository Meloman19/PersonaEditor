using System;
using System.Collections.Generic;
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

namespace PersonaEditorLib.Controls.Hex
{
    public partial class HexEditor : UserControl
    {
        public static readonly DependencyProperty StreamProperty = DependencyProperty.Register("Stream", typeof(Stream), typeof(HexEditor), new PropertyMetadata(null, StreamPropertyChange));

        private static void StreamPropertyChange(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }

        public Stream Stream
        {
            get { return (Stream)GetValue(StreamProperty); }
            set { SetValue(StreamProperty, value); }
        }
        
        public HexEditor()
        {
            InitializeComponent();
        }

        
    }
}