using AuxiliaryLibraries.WPF;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace PersonaEditorGUI.Controls
{
    public class ImagePreviewVM : BindingObject
    {
        private ImageSource imageSource = null;
        public ImageSource SourceIMG
        {
            get { return imageSource; }
            set
            {
                if (imageSource != value)
                {
                    imageSource = value;
                    Notify("SourceIMG");
                }
            }
        }

        private Color background;
        public Color Background
        {
            get { return background; }
            set
            {
                if (background != value)
                {
                    background = value;
                    Settings.AppSetting.Default.PreviewSelectedColor = value;
                    Notify("Background");
                }
            }
        }

        public ICommand SelectBack { get; }
        private void SelectBackground()
        {
            ColorPicker.ColorPickerTool colorPickerTool = new ColorPicker.ColorPickerTool(Background);
            if (colorPickerTool.ShowDialog() == true)
                Background = colorPickerTool.Color;
        }

        public ImagePreviewVM()
        {
            background = Settings.AppSetting.Default.PreviewSelectedColor;
            SelectBack = new RelayCommand(SelectBackground);
        }        
    }
}