using AuxiliaryLibraries.WPF;
using PersonaEditor.Common;
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
            get => imageSource;
            set => SetProperty(ref imageSource, value);
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
            ColorPickerTool colorPickerTool = new ColorPickerTool()
            {
                SelectedColor = Background,
            };
            if (colorPickerTool.ShowDialog() == true)
                Background = colorPickerTool.SelectedColor;
        }

        public ImagePreviewVM()
        {
            background = ApplicationSettings.AppSetting.Default.PreviewSelectedColor;
            SelectBack = new RelayCommand(SelectBackground);
        }        
    }
}