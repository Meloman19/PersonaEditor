using AuxiliaryLibraries.WPF;
using PersonaEditor.Views.Tools;
using System.Windows.Input;
using System.Windows.Media;

namespace PersonaEditor.ViewModels
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
                    ApplicationSettings.AppSetting.Default.PreviewSelectedColor = value;
                    Notify("Background");
                }
            }
        }

        public ICommand SelectBack { get; }
        private void SelectBackground()
        {
            ColorPickerTool colorPickerTool = new ColorPickerTool(Background);
            if (colorPickerTool.ShowDialog() == true)
                Background = colorPickerTool.Color;
        }

        public ImagePreviewVM()
        {
            background = ApplicationSettings.AppSetting.Default.PreviewSelectedColor;
            SelectBack = new RelayCommand(SelectBackground);
        }        
    }
}