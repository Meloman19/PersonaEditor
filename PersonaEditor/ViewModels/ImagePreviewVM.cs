using System.Windows.Input;
using System.Windows.Media;
using PersonaEditor.Common;
using PersonaEditor.Common.Settings;
using PersonaEditor.Views.Tools;

namespace PersonaEditor.ViewModels
{
    public sealed class ImagePreviewVM : BindingObject
    {
        private readonly SettingsProvider _settingsProvider;

        private ImageSource imageSource = null;
        private Color _background;

        public ImagePreviewVM()
        {
            _settingsProvider = Static.SettingsProvider;
            _background = _settingsProvider.AppSettings.PreviewSelectedColor;
            SelectBackgroundCommand = new RelayCommand(SelectBackground);
        }

        public ImageSource SourceIMG
        {
            get => imageSource;
            set => SetProperty(ref imageSource, value);
        }

        public Color Background
        {
            get { return _background; }
            set
            {
                if (SetProperty(ref _background, value))
                {
                    _settingsProvider.AppSettings.PreviewSelectedColor = value;
                }
            }
        }

        public ICommand SelectBackgroundCommand { get; }

        private void SelectBackground()
        {
            ColorPickerTool colorPickerTool = new ColorPickerTool()
            {
                SelectedColor = Background,
            };
            if (colorPickerTool.ShowDialog() == true)
                Background = colorPickerTool.SelectedColor;
        }
    }
}