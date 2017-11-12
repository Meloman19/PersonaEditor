using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
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

namespace PersonaEditorLib.ColorPicker
{
    public partial class ColorPickerTool : Window, INotifyPropertyChanged
    {
        #region INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;

        protected void Notify(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion INotifyPropertyChanged implementation

        private Color _Color = Colors.Transparent;
        public Color Color
        {
            get { return _Color; }
            set
            {
                if (value != _Color)
                {
                    _Color = value;
                    Notify("Color");
                }
            }
        }
        
        public ColorPickerTool(Color color = new Color())
        {
            InitializeComponent();
            Color = color;
            CanvasRGBUC.SelectColorChanged += CanvasRGBUC_SelectColorChanged;
        }

        private void CanvasRGBUC_SelectColorChanged(Color color)
        {
            Color temp = new Color()
            {
                A = Color.A,
                R = color.R,
                G = color.G,
                B = color.B
            };
            Color = temp;
        }

        private void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
