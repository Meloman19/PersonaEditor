using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace PersonaEditor.Classes
{
    class VisualText : UserControl
    {
        #region DependencyProperty Encoding

        public static DependencyProperty EncodingProperty = DependencyProperty.Register("Encoding", typeof(Encoding), typeof(VisualText), new PropertyMetadata(null, EncodingPropertyChanged));

        private static void EncodingPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }

        public Encoding Encoding
        {
            get { return (Encoding)GetValue(EncodingProperty); }
            set { SetValue(EncodingProperty, value); }
        }

        #endregion



        Image image = new Image();

        public VisualText()
        {
            ClipToBounds = true;
            base.AddChild(image);
        }

        protected override void AddChild(object value)
        {
        }
    }
}