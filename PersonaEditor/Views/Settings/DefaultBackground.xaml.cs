using System.Windows;
using System.Windows.Controls;

namespace PersonaEditor.View.Settings
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