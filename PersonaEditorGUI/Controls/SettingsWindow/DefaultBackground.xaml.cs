using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace PersonaEditorGUI.Controls.SettingsWindow
{
    public partial class DefaultBackground : UserControl
    {
        public DefaultBackground()
        {
            InitializeComponent();
        }

        private void TextColor_Pick(object sender, RoutedEventArgs e)
        {
            //var a = ColorConverter.ConvertFromString(TextColor);
            //PersonaEditorLib.ColorPicker.ColorPickerTool colorPicker = new PersonaEditorLib.ColorPicker.ColorPickerTool((Color)a);
            //if (colorPicker.ShowDialog() == true)
            //    TextColor = colorPicker.Color.ToString();
        }

        private void NameColor_Pick(object sender, RoutedEventArgs e)
        {
            //var a = ColorConverter.ConvertFromString(NameColor);
            //PersonaEditorLib.ColorPicker.ColorPickerTool colorPicker = new PersonaEditorLib.ColorPicker.ColorPickerTool((Color)a);
            //if (colorPicker.ShowDialog() == true)
            //    NameColor = colorPicker.Color.ToString();
        }

        private void BackgroundColor_Pick(object sender, RoutedEventArgs e)
        {
            //var a = ColorConverter.ConvertFromString(BackgroundColor);
            //PersonaEditorLib.ColorPicker.ColorPickerTool colorPicker = new PersonaEditorLib.ColorPicker.ColorPickerTool((Color)a);
            //if (colorPicker.ShowDialog() == true)
            //    BackgroundColor = colorPicker.Color.ToString();
        }
    }
}